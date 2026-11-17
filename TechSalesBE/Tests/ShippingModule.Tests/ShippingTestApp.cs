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
using ShippingModule.Infrastructure.Persistence;
using ShippingModule.Infrastructure.Persistence.Models;
using ShippingRegistration = ShippingModule.Presentation.ModuleRegistration.ModuleRegistration;

namespace ShippingModule.Tests;

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

internal sealed class ShippingTestApp : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;
    public SaveFailureInterceptor SaveFailure { get; }

    private ShippingTestApp(WebApplication app, SqliteConnection connection, SaveFailureInterceptor saveFailure)
    {
        _app = app;
        _connection = connection;
        _client = app.GetTestClient();
        SaveFailure = saveFailure;
    }

    public static async Task<ShippingTestApp> CreateAsync()
    {
        SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SHIPPING:DbConnectionString"] = "Host=unused;Database=unused"
        });
        ShippingRegistration.RegisterModule(builder.Services, builder.Configuration);
        builder.Services.RemoveAll<DbContextOptions<ShippingDbContext>>();
        builder.Services.RemoveAll<IDbContextOptionsConfiguration<ShippingDbContext>>();
        SaveFailureInterceptor saveFailure = new SaveFailureInterceptor();
        builder.Services.AddDbContext<ShippingDbContext>(options =>
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
            ShippingDbContext db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
            await db.Database.EnsureCreatedAsync();
            // Synthetic data exists only in this in-memory database.
            db.Provices.AddRange(
                new ProvinceDbModel { Code = "11", Name = "Test province A" },
                new ProvinceDbModel { Code = "44", Name = "Test province B" });
            db.Wards.AddRange(
                new WardDbModel { Code = "00001", Name = "Test ward A", ProvinceCode = "11" },
                new WardDbModel { Code = "00002", Name = "Test ward B", ProvinceCode = "44" });
            await db.SaveChangesAsync();
        }

        return new ShippingTestApp(app, connection, saveFailure);
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

    public async Task<T> WithDatabaseAsync<T>(Func<ShippingDbContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = _app.Services.CreateAsyncScope();
        ShippingDbContext db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
        return await action(db);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
