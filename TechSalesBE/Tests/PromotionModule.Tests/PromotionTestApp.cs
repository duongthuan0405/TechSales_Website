using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using CommonModule.Presentation.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromotionModule.Infrastructure.Persistence;
using PromotionModule.Infrastructure.Persistence.Models;
using PromotionRegistration = PromotionModule.Presentation.ModuleRegistration.ModuleRegistration;

namespace PromotionModule.Tests;

internal sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string subject = Request.Headers["X-Test-Actor"].ToString();
        if (!Guid.TryParse(subject, out Guid actorId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        List<Claim> claims = new List<Claim> { new Claim("sub", actorId.ToString()) };
        string role = Request.Headers["X-Test-Role"].ToString();
        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim("role", role));
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name, "sub", "role");
        AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

internal sealed class SqliteModelCustomizer : ModelCustomizer
{
    public SqliteModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        ValueConverter<DateTimeOffset, long> converter = new ValueConverter<DateTimeOffset, long>(
            value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in entity.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(converter);
                    property.SetColumnType("INTEGER");
                    property.SetDefaultValueSql(null);
                }
            }
        }
    }
}

internal sealed class SaveFailureInterceptor : SaveChangesInterceptor
{
    public bool FailWrites { get; set; }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (FailWrites)
        {
            throw new InvalidOperationException("Simulated storage failure.");
        }

        return ValueTask.FromResult(result);
    }
}

internal sealed class PromotionTestApp : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;
    public SaveFailureInterceptor SaveFailure { get; }

    private PromotionTestApp(WebApplication app, SqliteConnection connection, SaveFailureInterceptor saveFailure)
    {
        _app = app;
        _connection = connection;
        _client = app.GetTestClient();
        SaveFailure = saveFailure;
    }

    public static async Task<PromotionTestApp> CreateAsync()
    {
        SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PROMOTION:DbConnectionString"] = "Host=unused;Database=unused"
        });
        PromotionRegistration.RegisterModule(builder.Services, builder.Configuration);
        builder.Services.RemoveAll<DbContextOptions<PromotionDbContext>>();
        builder.Services.RemoveAll<IDbContextOptionsConfiguration<PromotionDbContext>>();
        SaveFailureInterceptor saveFailure = new SaveFailureInterceptor();
        builder.Services.AddDbContext<PromotionDbContext>(options =>
        {
            options.UseSqlite(connection);
            options.ReplaceService<IModelCustomizer, SqliteModelCustomizer>();
            options.AddInterceptors(saveFailure);
        });
        builder.Services.AddTransient<GlobalExceptionMiddleware>();
        builder.Services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        builder.Services.AddAuthorization();
        WebApplication app = builder.Build();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            PromotionDbContext db = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
            await db.Database.EnsureCreatedAsync();

        }

        return new PromotionTestApp(app, connection, saveFailure);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, Guid? actor = null, object? body = null, string? role = null)
    {
        using HttpRequestMessage request = new HttpRequestMessage(method, path);
        if (actor.HasValue)
        {
            request.Headers.Add("X-Test-Actor", actor.Value.ToString());
        }

        if (role != null)
        {
            request.Headers.Add("X-Test-Role", role);
        }

        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await _client.SendAsync(request);
    }

    public async Task<T> WithDatabaseAsync<T>(Func<PromotionDbContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = _app.Services.CreateAsyncScope();
        PromotionDbContext db = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
        return await action(db);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
