using System.Globalization;
using System.Text;
using System.Text.Json;
using Npgsql;
using Xunit;

namespace InventoryModule.Tests.Integration;

public sealed partial class InventoryHttpTests : IClassFixture<InventoryTestApp>
{
    private readonly InventoryTestApp _app;

    public InventoryHttpTests(InventoryTestApp app)
    {
        _app = app;
    }

    private Task<InventoryHttpResult> Receive(Guid product, string?[] serials, string? token) =>
        _app.SendAsync(HttpMethod.Post, ReceivePath(product), token, new { serialNumbers = serials });

    private Task<InventoryHttpResult> Damage(Guid product, string? serial, string? target, string? token) =>
        _app.SendAsync(HttpMethod.Patch, DamagePath(product), token, new { serialNumber = serial, status = target });

    private static string ReceivePath(Guid product) => $"/api/inventory/{product}/receive";

    private static string DamagePath(Guid product) => $"/api/inventory/{product}/units/status";

    private Task<InventoryHttpResult> Import(
        byte[]? contents, string? token, string name = "inventory.xlsx", bool expectContinue = false)
    {
        var form = new MultipartFormDataContent();
        if (contents == null)
        {
            form.Add(new StringContent("fixture"), "otherField");
        }
        else
        {
            form.Add(new ByteArrayContent(contents), "file", name);
        }

        return _app.SendAsync(HttpMethod.Post, "/api/inventory/import", token, form, expectContinue);
    }

    private async Task AssertRestrictedAsync(Func<string?, Task<InventoryHttpResult>> send)
    {
        Error(await send(null), 401, "UNAUTHORIZED");
        Error(await send(_app.ExpiredToken), 401, "UNAUTHORIZED");
        Error(await send(_app.InvalidSignatureToken), 401, "UNAUTHORIZED");
        Error(await send(_app.Customer), 403, "FORBIDDEN");
    }

    private static void Ok(InventoryHttpResult result, string expectedMessage)
    {
        Assert.Equal(200, result.Code);
        Assert.True(result.Json.GetProperty("success").GetBoolean());
        Assert.Equal(expectedMessage, result.Json.GetProperty("message").GetString());
    }

    private static void Error(
        InventoryHttpResult result, int status, string code, string? field = null,
        string? message = null, string? details = null)
    {
        Assert.Equal(status, result.Code);
        Assert.Equal(code, result.Json.GetProperty("code").GetString());
        Assert.False(result.Json.GetProperty("success").GetBoolean());
        if (field != null)
        {
            Assert.True(result.Data.TryGetProperty(field, out var errors), $"Expected an error for {field}.");
            Assert.NotEmpty(errors.EnumerateArray());
            if (details != null)
            {
                Assert.Contains(errors.EnumerateArray(), error => error.GetString() == details);
            }
        }
        if (message != null)
        {
            Assert.Equal(message, result.Json.GetProperty("message").GetString());
        }
    }

    private async Task<(int Quantity, int? Reserved)?> Stock(Guid productId)
    {
        await using NpgsqlConnection db = await _app.OpenInventoryAsync();
        await using NpgsqlCommand command = InventoryTestApp.CreateCommand(db,
            """SELECT "Quantity", "ReservedQuantity" FROM inventory."Invetory" WHERE "ProductId" = $1""", [productId]);
        await using NpgsqlDataReader result = await command.ExecuteReaderAsync();
        if (!await result.ReadAsync())
        {
            return null;
        }

        int? reserved = await result.IsDBNullAsync(1) ? null : result.GetInt32(1);
        return (result.GetInt32(0), reserved);
    }

    private Task<string?> Status(Guid productId, string serial) => ReadScalarAsync<string?>(
        """SELECT "Status" FROM inventory."ProductUnit" WHERE "ProductId" = $1 AND "SerialNumber" = $2""",
        [productId, serial]);

    private async Task<int> UnitCount(Guid productId) => Convert.ToInt32(await ReadScalarAsync<long>(
        """SELECT count(*) FROM inventory."ProductUnit" WHERE "ProductId" = $1""", [productId]), CultureInfo.InvariantCulture);

    private async Task<int> TotalUnits() => Convert.ToInt32(await ReadScalarAsync<long>(
        """SELECT count(*) FROM inventory."ProductUnit" """, []), CultureInfo.InvariantCulture);

    private async Task<T> ReadScalarAsync<T>(string sql, object?[] parameters)
    {
        await using NpgsqlConnection db = await _app.OpenInventoryAsync();
        await using NpgsqlCommand command = InventoryTestApp.CreateCommand(db, sql, parameters);
        return (T)(await command.ExecuteScalarAsync())!;
    }

    private Task<int> SeedStock(Guid productId, int quantity, int? reserved) => ExecuteSqlAsync(
        """INSERT INTO inventory."Invetory" ("ProductId", "Quantity", "ReservedQuantity") VALUES ($1, $2, $3)""",
        [productId, quantity, reserved]);

    private Task<int> SeedUnit(Guid productId, string serial, string status) => ExecuteSqlAsync(
        """INSERT INTO inventory."ProductUnit" ("ProductId", "SerialNumber", "Status") VALUES ($1, $2, $3)""",
        [productId, serial, status]);

    private async Task<int> ExecuteSqlAsync(string sql, object?[] parameters)
    {
        await using NpgsqlConnection db = await _app.OpenInventoryAsync();
        return await InventoryTestApp.ExecuteAsync(db, sql, parameters);
    }

    private static string Repeat(string value, int count) => string.Concat(Enumerable.Repeat(value, count));
}
