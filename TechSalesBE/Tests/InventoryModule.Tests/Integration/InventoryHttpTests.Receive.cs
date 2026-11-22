using System.Text;
using Xunit;

namespace InventoryModule.Tests.Integration;

public sealed partial class InventoryHttpTests
{
    [InventoryPostgresFact(DisplayName = "INV-REC-01 CreatesStockAndRegistersOneUnitPerSerial")]
    public async Task CreatesStockAndRegistersOneUnitPerSerial()
    {
        var one = Guid.NewGuid();
        var response = await Receive(one, ["R1", "R2"], _app.Staff);
        Ok(response, "Stock received.");
        Assert.Equal(one, response.Data.GetProperty("productId").GetGuid());
        Assert.Equal(2, response.Data.GetProperty("receivedQuantity").GetInt32());
        Assert.Equal(2, response.Data.GetProperty("quantity").GetInt32());
        Assert.Equal("IN_STOCK", response.Data.GetProperty("status").GetString());
        Assert.Equal((2, (int?)0), await Stock(one));
        Assert.Equal(2, await UnitCount(one));
        Assert.Equal("IN_STOCK", await Status(one, "R1"));
        Assert.Equal("IN_STOCK", await Status(one, "R2"));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-02 AddsToExistingStockWithoutChangingReservations")]
    public async Task AddsToExistingStockWithoutChangingReservations()
    {
        foreach (int? reserved in new int?[] { 3, null })
        {
            Guid id = Guid.NewGuid();
            await SeedStock(id, 10, reserved);
            Ok(await Receive(id, ["R1", "R2"], _app.Staff), "Stock received.");
            Assert.Equal((12, reserved), await Stock(id));
        }
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-03 AcceptsTheSameSerialForDifferentProducts")]
    public async Task AcceptsTheSameSerialForDifferentProducts()
    {
        var two = Guid.NewGuid();
        var one = Guid.NewGuid();
        Ok(await Receive(one, ["R1", "R2"], _app.Staff), "Stock received.");
        Ok(await Receive(two, ["R1"], _app.BusinessAdmin), "Stock received.");
        Assert.Equal(2, await UnitCount(one));
        Assert.Equal("IN_STOCK", await Status(one, "R1"));
        Assert.Equal("IN_STOCK", await Status(two, "R1"));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-04 RejectsMissingNullAndEmptySerialLists")]
    public async Task RejectsMissingNullAndEmptySerialLists()
    {
        var none = Guid.NewGuid();
        foreach (object body in new object[] { new { }, new { serialNumbers = (string[]?)null }, new { serialNumbers = Array.Empty<string>() } })
        {
            Error(await _app.SendAsync(HttpMethod.Post, ReceivePath(none), _app.Staff, body), 400, "BAD_REQUEST", "SerialNumbers");
        }
        Assert.True(await Stock(none) == null, "invalid lists write no stock");
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-05 ValidatesSerialLengthAndContent")]
    public async Task ValidatesSerialLengthAndContent()
    {
        var none = Guid.NewGuid();
        foreach (string? invalid in new string?[] { null, "", "  ", "ABC\0", Repeat("😀", 101) })
        {
            Error(await Receive(none, [invalid], _app.Staff), 400, "BAD_REQUEST", "SerialNumbers[0]");
        }
        var edge = Guid.NewGuid();
        Ok(await Receive(edge, [Repeat("😀", 100), "  MiXeD  "], _app.Staff), "Stock received.");
        Assert.Equal("IN_STOCK", await Status(edge, "  MiXeD  "));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-06 UsesOrdinalComparisonToFindDuplicateSerials")]
    public async Task UsesOrdinalComparisonToFindDuplicateSerials()
    {
        var none = Guid.NewGuid();
        Error(await Receive(none, ["repeat", "repeat"], _app.Staff), 400, "BAD_REQUEST", "SerialNumbers");
        Assert.True(await Stock(none) == null, "rejection before write");
        var edge = Guid.NewGuid();
        Ok(await Receive(edge, ["MiXeD", "mixed"], _app.Staff), "Stock received.");
        Assert.Equal(2, await UnitCount(edge));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-07 DistinguishesMalformedAndEmptyProductIds")]
    public async Task DistinguishesMalformedAndEmptyProductIds()
    {
        Error(await Receive(Guid.Empty, ["R"], _app.Staff), 400, "BAD_REQUEST", "ProductId");
        Assert.Equal(404, (await _app.SendAsync(HttpMethod.Post, "/api/inventory/not-a-uuid/receive", _app.Staff, new { serialNumbers = new[] { "R" } })).Code);
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-08 RollsBackTheWholeBatchWhenOneSerialExists")]
    public async Task RollsBackTheWholeBatchWhenOneSerialExists()
    {
        var one = Guid.NewGuid();
        Ok(await Receive(one, ["R1", "R2"], _app.Staff), "Stock received.");
        Error(await Receive(one, ["R-NEW", "R1"], _app.Staff), 409, "CONFLICT");
        Assert.Equal((2, (int?)0), await Stock(one));
        Assert.True(await Status(one, "R-NEW") == null, "new serial rolled back");
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-09 RepeatingAReceiptLeavesStockUnchanged")]
    public async Task RepeatingAReceiptLeavesStockUnchanged()
    {
        var one = Guid.NewGuid();
        Ok(await Receive(one, ["R1", "R2"], _app.Staff), "Stock received.");
        Error(await Receive(one, ["R1", "R2"], _app.Staff), 409, "CONFLICT");
        Assert.Equal((2, (int?)0), await Stock(one));
        Assert.Equal(2, await UnitCount(one));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-10 SerializesConcurrentReceiptsForTheSameProduct")]
    public async Task SerializesConcurrentReceiptsForTheSameProduct()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, 10, 0);
        await using var db = await _app.OpenInventoryAsync();
        await using var blocker = await db.BeginTransactionAsync();
        await InventoryTestApp.ExecuteAsync(db, """SELECT 1 FROM inventory."Invetory" WHERE "ProductId"=$1 FOR UPDATE""", [id], blocker);
        var first = Receive(id, ["A", "B"], _app.Staff);
        var second = Receive(id, ["C", "D", "E"], _app.Staff);
        await _app.WaitForLockAsync();
        await blocker.CommitAsync();
        foreach (var response in await Task.WhenAll(first, second))
        {
            Ok(response, "Stock received.");
        }
        Assert.Equal((15, (int?)0), await Stock(id));
        Assert.Equal(5, await UnitCount(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-11 RejectsStockQuantityOverflow")]
    public async Task RejectsStockQuantityOverflow()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, int.MaxValue, 0);
        Error(await Receive(id, ["R"], _app.Staff), 422, "INVENTORY.STOCK_QUANTITY_LIMIT_EXCEEDED");
        Assert.Equal((int.MaxValue, (int?)0), await Stock(id));
        Assert.Equal(0, await UnitCount(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-12 RejectsUnauthorizedAndForbiddenReceipts")]
    public async Task RejectsUnauthorizedAndForbiddenReceipts()
    {
        var none = Guid.NewGuid();
        await AssertRestrictedAsync(credential => Receive(none, ["R"], credential));
        Assert.True(await Stock(none) == null, "no stock writes");
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-13 RejectsInvalidJsonAndUnsupportedMediaTypes")]
    public async Task RejectsInvalidJsonAndUnsupportedMediaTypes()
    {
        var none = Guid.NewGuid();
        Error(await _app.SendAsync(HttpMethod.Post, ReceivePath(none), _app.Staff, new StringContent("{", Encoding.UTF8, "application/json")), 400, "BAD_REQUEST");
        Assert.Equal(415, (await _app.SendAsync(HttpMethod.Post, ReceivePath(none), _app.Staff, new StringContent("plain", Encoding.UTF8, "text/plain"))).Code);
        Assert.True(await Stock(none) == null, "invalid HTTP body no writes");
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-14 RejectsStockWithANegativeQuantity")]
    public async Task RejectsStockWithANegativeQuantity()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, -1, 0);
        Error(await Receive(id, ["R"], _app.Staff), 422, "INVENTORY.INVALID_STOCK_QUANTITY");
        Assert.Equal((-1, (int?)0), await Stock(id));
        Assert.Equal(0, await UnitCount(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-15 AcceptsStockQuantityAtTheIntegerLimit")]
    public async Task AcceptsStockQuantityAtTheIntegerLimit()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, int.MaxValue - 1, 0);
        Ok(await Receive(id, ["EDGE"], _app.Staff), "Stock received.");
        Assert.Equal((int.MaxValue, (int?)0), await Stock(id));
        Assert.Equal(1, await UnitCount(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-REC-16 RollsBackNewStockWhenAnExistingUnitConflicts")]
    public async Task RollsBackNewStockWhenAnExistingUnitConflicts()
    {
        var id = Guid.NewGuid();
        await SeedUnit(id, "EXISTS", "IN_STOCK");
        Error(await Receive(id, ["EXISTS", "NOT-WRITTEN"], _app.Staff), 409, "CONFLICT");
        Assert.True(await Stock(id) == null, "new stock rolled back on conflict");
        Assert.Equal(1, await UnitCount(id));
        Assert.True(await Status(id, "NOT-WRITTEN") == null, "new serial rolled back");
    }
}
