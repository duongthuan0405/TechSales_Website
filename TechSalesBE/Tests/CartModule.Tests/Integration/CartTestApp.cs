using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Services;
using CartModule.Infrastructure.Persistence;
using CommonModule.Presentation.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Xunit;
using AuthRegistration = AuthModule.Presentation.ModuleRegistration.ModuleRegistration;
using CartRegistration = CartModule.Presentation.ModuleRegistration.ModuleRegistration;
using CommonRegistration = CommonModule.Presentation.ModuleRegistration.ModuleRegistration;

namespace CartModule.Tests.Integration;

public sealed class CartTestApp : IAsyncLifetime
{
    private const string Issuer = "CartTests";
    private const string Audience = "CartTestsClient";
    private readonly byte[] _signingKey = RandomNumberGenerator.GetBytes(32);
    private readonly string _password = "Test!" + Guid.NewGuid().ToString("N");
    private readonly string _database = "cart_test_" + Guid.NewGuid().ToString("N");
    private const string ApplicationName = "cart_api_fixture";
    private string _maintenanceConnection = null!;
    private string _databaseConnection = null!;
    private bool _databaseCreated;
    private WebApplication? _app;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        string? setting = Environment.GetEnvironmentVariable(CartPostgresFactAttribute.ConnectionVariable);
        if (string.IsNullOrWhiteSpace(setting))
        {
            return;
        }

        var connection = new NpgsqlConnectionStringBuilder(setting) { Pooling = false };
        _maintenanceConnection = connection.ConnectionString;
        connection.Database = _database;
        connection.ApplicationName = ApplicationName;
        _databaseConnection = connection.ConnectionString;

        try
        {
            await using (var admin = await OpenAsync(_maintenanceConnection))
            {
                await using var create = new NpgsqlCommand($"""CREATE DATABASE "{_database}" """, admin);
                await create.ExecuteNonQueryAsync();
                _databaseCreated = true;
            }

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
            builder.Logging.ClearProviders();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH:DbConnectionString"] = _databaseConnection,
                ["AUTH:JWT:SecretKey"] = Convert.ToBase64String(_signingKey),
                ["AUTH:JWT:Issuer"] = Issuer,
                ["AUTH:JWT:Audience"] = Audience,
                ["AUTH:RateLimit:PermitLimit"] = "1000",
                ["CART:DbConnectionString"] = _databaseConnection
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            CommonRegistration.RegisterModule(builder.Services, builder.Configuration);
            AuthRegistration.RegisterModule(builder.Services, builder.Configuration);
            CartRegistration.RegisterModule(builder.Services, builder.Configuration);
            builder.Services.AddSingleton<CartModule.Application.Ports.ICartProducts, CartProductsFake>();

            _app = builder.Build();
            await using (var scope = _app.Services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.EnsureCreatedAsync();
                await scope.ServiceProvider.GetRequiredService<CartDbContext>().Database
                    .MigrateAsync();
            }

            _app.UseMiddleware<GlobalExceptionMiddleware>();
            _app.UseRouting();
            _app.UseAuthentication();
            _app.UseRateLimiter();
            _app.UseAuthorization();
            _app.MapControllers();
            await _app.StartAsync();
            _client = new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                BaseAddress = new Uri(_app.Urls.Single()),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        try
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
                _app = null;
            }
        }
        finally
        {
            if (_databaseCreated)
            {
                await using var admin = await OpenAsync(_maintenanceConnection);
                await using var drop = new NpgsqlCommand($"""DROP DATABASE "{_database}" """, admin);
                await drop.ExecuteNonQueryAsync();
                _databaseCreated = false;
            }
        }
    }

    internal async Task<CartActor> CreateActorAsync(string role = "Customer")
    {
        Guid id = Guid.NewGuid();
        string email = $"{id:N}@example.test";
        await ExecuteAsync("""
            INSERT INTO identify_access."User" ("Id", "Email", "HashedPassword", "Status")
            VALUES ($1, $2, $3, 'ACTIVE')
            """, id, email, new PasswordHasher().HashPassword(_password));
        Assert.Equal(1, await ExecuteAsync("""
            INSERT INTO identify_access."UserRole" ("UserId", "RoleId")
            SELECT $1, "Id" FROM identify_access."Role" WHERE "Name" = $2
            """, id, role));

        CartHttpResult login = await SendAsync(HttpMethod.Post, "/api/auth/login", null, new { email, password = _password });
        Assert.Equal(200, login.Code);
        return new CartActor(id, login.Data.GetProperty("token").GetString()!);
    }

    internal string CreateToken(string? subject, bool expired = false)
    {
        DateTime now = DateTime.UtcNow;
        var token = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            IssuedAt = now.AddMinutes(-10),
            NotBefore = now.AddMinutes(-10),
            Expires = expired ? now.AddMinutes(-5) : now.AddMinutes(5),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_signingKey), SecurityAlgorithms.HmacSha256)
        };
        if (subject != null)
        {
            token.Claims = new Dictionary<string, object> { ["sub"] = subject };
        }
        return new JsonWebTokenHandler().CreateToken(token);
    }

    internal async Task<CartHttpResult> SendAsync(HttpMethod method, string path, string? token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        request.Content = body as HttpContent ?? (body == null ? null : JsonContent.Create(body));
        using HttpResponseMessage response = await _client!.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        JsonElement json = default;
        if (content.TrimStart().StartsWith('{'))
        {
            using JsonDocument document = JsonDocument.Parse(content);
            json = document.RootElement.Clone();
        }
        return new CartHttpResult((int)response.StatusCode, json, response.Headers.CacheControl?.NoStore == true);
    }

    internal Task<NpgsqlConnection> OpenAsync()
    {
        return OpenAsync(_databaseConnection);
    }

    internal async Task<int> ExecuteAsync(string sql, params object[] parameters)
    {
        await using NpgsqlConnection connection = await OpenAsync();
        await using NpgsqlCommand command = Command(connection, sql, parameters);
        return await command.ExecuteNonQueryAsync();
    }

    internal async Task WaitForCartLockAsync()
    {
        await using NpgsqlConnection connection = await OpenAsync();
        await using NpgsqlCommand command = Command(connection, """
            SELECT EXISTS (
                SELECT FROM pg_stat_activity
                WHERE datname = current_database() AND application_name = $1 AND wait_event_type = 'Lock'
            )
            """, [ApplicationName]);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while ((bool)(await command.ExecuteScalarAsync(timeout.Token))! == false)
        {
            await Task.Delay(20, timeout.Token);
        }
    }

    internal static NpgsqlCommand Command(NpgsqlConnection connection, string sql, object[] parameters)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (object value in parameters)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }
        return command;
    }

    private static async Task<NpgsqlConnection> OpenAsync(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}

internal sealed record CartActor(Guid Id, string Token);

internal sealed record CartHttpResult(int Code, JsonElement Json, bool NoStore)
{
    internal JsonElement Data => Json.GetProperty("data");
}
