using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Infrastructure.Persistence;
using CommonModule.Presentation.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using AuthRegistration = AuthModule.Presentation.ModuleRegistration.ModuleRegistration;
using CommonRegistration = CommonModule.Presentation.ModuleRegistration.ModuleRegistration;

namespace AuthModule.Tests;
internal sealed class CapturedMail : IAuthEmailAdapter
{
    public ConcurrentDictionary<string, string> Verification { get; } = new();
    public ConcurrentDictionary<string, string> Reset { get; } = new();
    public bool Fail { get; set; }

    public Task SendVerificationAsync(string email, string token, CancellationToken cancellationToken)
    {
        if (Fail)
        {
            throw new IOException("Simulated mail failure.");
        }

        Verification[email] = token;
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken)
    {
        if (Fail)
        {
            throw new IOException("Simulated mail failure.");
        }

        Reset[email] = token;
        return Task.CompletedTask;
    }
}

// SQLite stores DateTimeOffset as text by default; use sortable numeric values only in tests.
internal sealed class SqliteTimeModelCustomizer : ModelCustomizer
{
    public SqliteTimeModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder builder, DbContext context)
    {
        base.Customize(builder, context);
        foreach (var property in builder.Model.GetEntityTypes().SelectMany(e => e.GetProperties()))
        {
            if ((Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == typeof(DateTimeOffset))
            {
                property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
            }
        }
    }
}

internal sealed class AuthTestApp : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private WebApplication _app = null!;
    private readonly string _root = Path.Combine(Path.GetTempPath(), "techsales-auth-tests", Guid.NewGuid().ToString("N"));
    internal HttpClient Client { get; private set; } = null!;
    internal CapturedMail Mail { get; } = new();

    internal const string Password = "Example!Password123";
    public static async Task<AuthTestApp> CreateAsync(Dictionary<string, string?>? extra = null)
    {
        var test = new AuthTestApp();
        Directory.CreateDirectory(test._root);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production", ContentRootPath = test._root, ApplicationName = typeof(AuthRegistration).Assembly.GetName().Name });
        builder.Logging.ClearProviders().AddConsole().SetMinimumLevel(LogLevel.Error);
        builder.WebHost.UseTestServer();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        var config = new Dictionary<string, string?>
        {
            ["AUTH:DbConnectionString"] = "Host=localhost;Database=not_used;Username=not_used;Password=not_used",
            ["AUTH:JWT:SecretKey"] = Convert.ToBase64String(Enumerable.Range(1, 32).Select(x => (byte)x).ToArray()),
            ["AUTH:JWT:Issuer"] = "AuthTests",
            ["AUTH:JWT:Audience"] = "AuthTestsClient"
        };
        if (extra is not null)
        {
            foreach (var pair in extra)
            {
                config[pair.Key] = pair.Value;
            }
        }

        builder.Configuration.AddInMemoryCollection(config);
        CommonRegistration.RegisterModule(builder.Services, builder.Configuration);
        AuthRegistration.RegisterModule(builder.Services, builder.Configuration);
        builder.Services.AddSingleton<IAuthEmailAdapter>(test.Mail);
        builder.Services.RemoveAll<DbContextOptions<AuthDbContext>>();
        builder.Services.RemoveAll<IDbContextOptionsConfiguration<AuthDbContext>>();
        builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlite(test._connection).ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>());
        await test._connection.OpenAsync();
        test._app = builder.Build();
        using (var scope = test._app.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.EnsureCreatedAsync();
        }

        test._app.UseMiddleware<GlobalExceptionMiddleware>();
        test._app.UseRouting();
        test._app.UseCors("auth-frontend");
        test._app.UseAuthentication();
        test._app.UseRateLimiter();
        test._app.UseAuthorization();
        test._app.MapControllers();
        await test._app.StartAsync();
        test.Client = test._app.GetTestClient();
        return test;
    }

    internal async Task<T> DbAsync<T>(Func<AuthDbContext, Task<T>> action)
    {
        using var scope = _app.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<AuthDbContext>());
    }

    internal async Task<TResponse> SendAsync<TResponse>(MediatR.IRequest<TResponse> input)
    {
        using IServiceScope scope = _app.Services.CreateScope();
        MediatR.ISender sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();
        return await sender.Send(input);
    }

    private static int _phoneSequence;
    internal static string NextPhone() => "0" + Interlocked.Increment(ref _phoneSequence).ToString("D9");

    internal async Task<Guid> SeedUserAsync(string email, string role, bool active = true)
    {
        using var scope = _app.Services.CreateScope();
        var provider = scope.ServiceProvider;
        var users = provider.GetRequiredService<IUserRepository>();
        var assigned = await provider.GetRequiredService<IRoleRepository>().GetByNameAsync(role);
        return await provider.GetRequiredService<IExecuteAtomically>().ExecuteAtomicallyAsync(async () =>
        {
            var user = User.Create(email, provider.GetRequiredService<IPasswordHasher>().HashPassword(Password), active);
            user.Profile.FullName = "Test User";
            user.Profile.PhoneNumber = NextPhone();
            user.Roles.Add(assigned!);
            await users.AddAsync(user);
            return user.Id;
        });
    }

    internal async Task<string> LoginAsync(string email, string? password = null)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, password = password ?? Password });
        var json = await ExpectAsync(response, HttpStatusCode.OK);
        return json.GetProperty("data").GetProperty("token").GetString()!;
    }

    internal HttpRequestMessage Authorized(HttpMethod method, string path, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    internal static async Task<JsonElement> ExpectAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expected, $"Expected {(int)expected}, got {(int)response.StatusCode}: {body}");
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
        var parent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "techsales-auth-tests")) + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(_root).StartsWith(parent, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Test cleanup path is outside the temporary test directory.");
        }

        Directory.Delete(_root, recursive: true);
    }
}
