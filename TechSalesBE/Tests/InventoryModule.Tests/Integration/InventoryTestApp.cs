using System.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Services;
using CommonModule.Presentation.Middlewares;
using InventoryModule.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit;
using AuthRegistration = AuthModule.Presentation.ModuleRegistration.ModuleRegistration;
using CommonRegistration = CommonModule.Presentation.ModuleRegistration.ModuleRegistration;
using InventoryRegistration = InventoryModule.Presentation.ModuleRegistration.ModuleRegistration;

namespace InventoryModule.Tests.Integration;

public sealed class InventoryTestApp : IAsyncLifetime
{
    private const string Issuer = "InventoryTests";
    private const string Audience = "InventoryTestsClient";
    private readonly byte[] _signingKey = RandomNumberGenerator.GetBytes(32);
    private readonly string _password = "Test!" + Guid.NewGuid().ToString("N");
    private readonly string _inventoryDatabase = "inventory_test_" + Guid.NewGuid().ToString("N");
    private readonly string _authDatabase = "auth_test_" + Guid.NewGuid().ToString("N");
    private readonly string _applicationName = "inventory_tests_" + Guid.NewGuid().ToString("N");
    private readonly List<string> _createdDatabases = [];
    private string _serverConnection = null!;
    private string _inventoryConnection = null!;
    private string _authConnection = null!;
    private WebApplication? _app;
    private HttpClient? _client;

    internal string Customer { get; private set; } = null!;
    internal string Staff { get; private set; } = null!;
    internal string BusinessAdmin { get; private set; } = null!;
    internal string TechnicalAdmin { get; private set; } = null!;
    internal string ExpiredToken { get; private set; } = null!;
    internal string InvalidSignatureToken { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        string? configured = Environment.GetEnvironmentVariable(InventoryPostgresFactAttribute.ConnectionVariable);
        if (string.IsNullOrWhiteSpace(configured))
        {
            return;
        }

        _serverConnection = new NpgsqlConnectionStringBuilder(configured) { Pooling = false }.ConnectionString;
        _authConnection = ForDatabase(_authDatabase);
        _inventoryConnection = ForDatabase(_inventoryDatabase, _applicationName);
        try
        {
            await using (NpgsqlConnection admin = await OpenAsync(_serverConnection))
            {
                foreach (string name in new[] { _authDatabase, _inventoryDatabase })
                {
                    await ExecuteAsync(admin, $"""CREATE DATABASE "{name}" """, []);
                    _createdDatabases.Add(name);
                }
            }

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Production"
            });
            builder.Logging.ClearProviders();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH:DbConnectionString"] = _authConnection,
                ["AUTH:JWT:SecretKey"] = Convert.ToBase64String(_signingKey),
                ["AUTH:JWT:Issuer"] = Issuer,
                ["AUTH:JWT:Audience"] = Audience,
                ["INVENTORY:DbConnectionString"] = _inventoryConnection
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            CommonRegistration.RegisterModule(builder.Services, builder.Configuration);
            AuthRegistration.RegisterModule(builder.Services, builder.Configuration);
            InventoryRegistration.RegisterModule(builder.Services, builder.Configuration);

            _app = builder.Build();
            await using (var scope = _app.Services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.EnsureCreatedAsync();
                await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
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
                Timeout = TimeSpan.FromSeconds(45)
            };

            (Guid customerId, string customer) = await CreateAccountAsync("Customer");
            Customer = customer;
            (_, Staff) = await CreateAccountAsync("Staff");
            (_, BusinessAdmin) = await CreateAccountAsync("Business Admin");
            (_, TechnicalAdmin) = await CreateAccountAsync("Technical Admin");
            ExpiredToken = CreateExpiredToken(customerId);
            int signatureStart = Customer.LastIndexOf('.') + 1;
            char replacement = Customer[signatureStart] == 'A' ? 'B' : 'A';
            InvalidSignatureToken = Customer[..signatureStart] + replacement + Customer[(signatureStart + 1)..];
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
        _client = null;
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
            if (_createdDatabases.Count > 0)
            {
                await using NpgsqlConnection admin = await OpenAsync(_serverConnection);
                foreach (string name in _createdDatabases.ToArray())
                {
                    await ExecuteAsync(admin, $"""DROP DATABASE "{name}" """, []);
                    _createdDatabases.Remove(name);
                }
            }
        }
    }

    internal async Task<(Guid UserId, string Token)> CreateAccountAsync(string role)
    {
        Guid id = Guid.NewGuid();
        string email = $"{id:N}@example.test";
        await using NpgsqlConnection db = await OpenAsync(_authConnection);
        await ExecuteAsync(db, """
            INSERT INTO identify_access."User" ("Id", "Email", "HashedPassword", "Status")
            VALUES ($1, $2, $3, 'ACTIVE')
            """, [id, email, new PasswordHasher().HashPassword(_password)]);
        Assert.Equal(1, await ExecuteAsync(db, """
            INSERT INTO identify_access."UserRole" ("UserId", "RoleId")
            SELECT $1, "Id" FROM identify_access."Role" WHERE "Name" = $2
            """, [id, role]));
        InventoryHttpResult response = await SendAsync(HttpMethod.Post, "/api/auth/login", null,
            new { email, password = _password });
        Assert.Equal(200, response.Code);
        Assert.Contains(response.Data.GetProperty("roles").EnumerateArray(), value => value.GetString() == role);
        return (id, response.Data.GetProperty("token").GetString()!);
    }

    internal async Task InactivateAccountAsync(Guid userId)
    {
        await using NpgsqlConnection db = await OpenAsync(_authConnection);
        await ExecuteAsync(db, """UPDATE identify_access."User" SET "Status" = 'INACTIVE' WHERE "Id" = $1""", [userId]);
    }

    internal async Task<InventoryHttpResult> SendAsync(
        HttpMethod method, string path, string? token, object? body = null, bool expectContinue = false)
    {
        using var request = new HttpRequestMessage(method, path);
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (expectContinue)
        {
            request.Headers.ExpectContinue = true;
        }
        request.Content = body as HttpContent ?? (body == null ? null : JsonContent.Create(body));

        using HttpResponseMessage response = await _client!.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        if (!content.TrimStart().StartsWith('{'))
        {
            return new InventoryHttpResult((int)response.StatusCode, default);
        }

        using JsonDocument json = JsonDocument.Parse(content);
        return new InventoryHttpResult((int)response.StatusCode, json.RootElement.Clone());
    }

    internal Task<NpgsqlConnection> OpenInventoryAsync()
    {
        return OpenAsync(_inventoryConnection);
    }

    internal async Task WaitForLockAsync()
    {
        await using NpgsqlConnection observer = await OpenInventoryAsync();
        await using var command = new NpgsqlCommand("""
            SELECT EXISTS (
                SELECT FROM pg_stat_activity WHERE application_name = $1 AND wait_event_type = 'Lock'
            )
            """, observer);
        command.Parameters.Add(new NpgsqlParameter { Value = _applicationName });
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(bool)(await command.ExecuteScalarAsync(timeout.Token))!)
        {
            await Task.Delay(20, timeout.Token);
        }
    }

    private string ForDatabase(string database, string? applicationName = null)
    {
        return new NpgsqlConnectionStringBuilder(_serverConnection)
        {
            Database = database,
            ApplicationName = applicationName
        }.ConnectionString;
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

    internal static async Task<int> ExecuteAsync(
        NpgsqlConnection connection, string sql, object?[] parameters, NpgsqlTransaction? transaction = null)
    {
        await using NpgsqlCommand command = CreateCommand(connection, sql, parameters, transaction);
        return await command.ExecuteNonQueryAsync();
    }

    internal static NpgsqlCommand CreateCommand(
        NpgsqlConnection connection, string sql, object?[] parameters, NpgsqlTransaction? transaction = null)
    {
        var command = new NpgsqlCommand(sql, connection, transaction);
        foreach (object? value in parameters)
        {
            var parameter = new NpgsqlParameter { Value = value ?? DBNull.Value };
            if (value == null)
            {
                parameter.DbType = DbType.Int32;
            }
            command.Parameters.Add(parameter);
        }
        return command;
    }

    private string CreateExpiredToken(Guid userId)
    {
        static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = DateTimeOffset.UtcNow;
        string header = Encode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        string payload = Encode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = userId.ToString(),
            iss = Issuer,
            aud = Audience,
            nbf = now.AddMinutes(-10).ToUnixTimeSeconds(),
            exp = now.AddMinutes(-5).ToUnixTimeSeconds()
        }));
        string signed = header + "." + payload;
        return signed + "." + Encode(HMACSHA256.HashData(_signingKey, Encoding.ASCII.GetBytes(signed)));
    }
}

internal sealed record InventoryHttpResult(int Code, JsonElement Json)
{
    public JsonElement Data => Json.GetProperty("data");
}
