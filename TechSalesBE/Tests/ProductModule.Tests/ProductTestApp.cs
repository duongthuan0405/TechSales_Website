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
using ProductModule.Infrastructure.Persistence;
using ProductModule.Infrastructure.Persistence.Models;
using ProductModule.Application.Ports;
using ProductModule.Domain.ValueObjects;
using ProductRegistration = ProductModule.Presentation.ModuleRegistration.ModuleRegistration;

namespace ProductModule.Tests;

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
    public Exception? Failure { get; set; }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (Failure != null)
        {
            throw Failure;
        }

        if (FailWrites)
        {
            throw new InvalidOperationException("Simulated storage failure.");
        }

        return ValueTask.FromResult(result);
    }
}

internal sealed class ProductTestApp : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;
    private readonly string _webRoot;
    public SaveFailureInterceptor SaveFailure { get; }
    internal TestProductReferenceData References { get; }
    public int ImageFileCount => Directory.Exists(Path.Combine(_webRoot, "product-images"))
        ? Directory.GetFiles(Path.Combine(_webRoot, "product-images")).Length : 0;

    private ProductTestApp(WebApplication app, SqliteConnection connection, SaveFailureInterceptor saveFailure,
        string webRoot, TestProductReferenceData references)
    {
        _app = app;
        _connection = connection;
        _client = app.GetTestClient();
        SaveFailure = saveFailure;
        _webRoot = webRoot;
        References = references;
    }

    public static async Task<ProductTestApp> CreateAsync()
    {
        SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        string webRoot = Path.Combine(Path.GetTempPath(), "TechSalesProductTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(webRoot);
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = webRoot });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PRODUCT:DbConnectionString"] = "Host=unused;Database=unused"
        });
        ProductRegistration.RegisterModule(builder.Services, builder.Configuration);
        TestProductReferenceData references = new TestProductReferenceData();
        builder.Services.AddSingleton<IProductReferenceData>(references);
        builder.Services.RemoveAll<DbContextOptions<ProductDbContext>>();
        builder.Services.RemoveAll<IDbContextOptionsConfiguration<ProductDbContext>>();
        SaveFailureInterceptor saveFailure = new SaveFailureInterceptor();
        builder.Services.AddDbContext<ProductDbContext>(options =>
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
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            ProductDbContext db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            await db.Database.EnsureCreatedAsync();

        }

        return new ProductTestApp(app, connection, saveFailure, webRoot, references);
    }

    public async Task<(Guid BrandId, Guid CategoryId)> AddReferencesAsync(string brandName, string categoryName)
    {
        Guid brandId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        // Legacy FK fixtures only; use cases consume the independent reference port.
        await WithDatabaseAsync(async db =>
        {
            db.Brands.Add(new BrandDbModel { Id = brandId, Name = brandName });
            db.Categorys.Add(new CategoryDbModel
            {
                Id = categoryId, Name = categoryName, CreatedAat = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            return true;
        });
        References.Brands.Add(new ProductReference(brandId, brandName));
        References.Categories.Add(new ProductReference(categoryId, categoryName));
        return (brandId, categoryId);
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

    public async Task<T> WithDatabaseAsync<T>(Func<ProductDbContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = _app.Services.CreateAsyncScope();
        ProductDbContext db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        return await action(db);
    }

    public async Task<HttpResponseMessage> SendMultipartAsync(string path, MultipartFormDataContent body,
        string? role = "Business Admin")
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);
        if (role != null)
        {
            request.Headers.Add("X-Test-Actor", Guid.NewGuid().ToString());
            request.Headers.Add("X-Test-Role", role);
        }

        request.Content = body;
        return await _client.SendAsync(request);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
        string testRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "TechSalesProductTests"))
            + Path.DirectorySeparatorChar;
        string resolvedWebRoot = Path.GetFullPath(_webRoot);
        if (resolvedWebRoot.StartsWith(testRoot, StringComparison.OrdinalIgnoreCase) && Directory.Exists(resolvedWebRoot))
        {
            Directory.Delete(resolvedWebRoot, recursive: true);
        }
    }
}
