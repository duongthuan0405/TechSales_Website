using System.Text;
using Xunit;

namespace InventoryModule.Tests.Integration;

public sealed partial class InventoryHttpTests
{
    [InventoryPostgresFact(DisplayName = "INV-STA-01 MarksOneUnitDamagedAndReducesAvailableStock")]
    public async Task MarksOneUnitDamagedAndReducesAvailableStock()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, 3, 2);
        await SeedUnit(id, "D", "IN_STOCK");
        var response = await Damage(id, "D", "DAMAGED", _app.Staff);
        Ok(response, "Product unit status updated.");
        Assert.Equal(id, response.Data.GetProperty("productId").GetGuid());
        Assert.Equal("D", response.Data.GetProperty("serialNumber").GetString());
        Assert.Equal("DAMAGED", response.Data.GetProperty("status").GetString());
        Assert.Equal(2, response.Data.GetProperty("quantity").GetInt32());
        Assert.Equal((2, (int?)2), await Stock(id));
        Assert.Equal("DAMAGED", await Status(id, "D"));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-02 AcceptsTheDamageTargetWithoutCaseSensitivity")]
    public async Task AcceptsTheDamageTargetWithoutCaseSensitivity()
    {
        var id = Guid.NewGuid();
        Ok(await Receive(id, ["D"], _app.BusinessAdmin), "Stock received.");
        var response = await Damage(id, "D", "damaged", _app.TechnicalAdmin);
        Ok(response, "Product unit status updated.");
        Assert.Equal("DAMAGED", response.Data.GetProperty("status").GetString());
        Assert.Equal((0, (int?)0), await Stock(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-03 RejectsUnsupportedOrMissingTargets")]
    public async Task RejectsUnsupportedOrMissingTargets()
    {
        var id = Guid.NewGuid();
        Ok(await Receive(id, ["D"], _app.BusinessAdmin), "Stock received.");
        foreach (var status in new string?[] { "RESERVED", "SOLD", " DAMAGED ", "", null })
        {
            Error(await Damage(id, "D", status, _app.Staff), 400, "BAD_REQUEST", "Status");
        }
        Assert.Equal("IN_STOCK", await Status(id, "D"));
        Assert.Equal((1, (int?)0), await Stock(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-04 RejectsInvalidSerials")]
    public async Task RejectsInvalidSerials()
    {
        var id = Guid.NewGuid();
        Ok(await Receive(id, ["D"], _app.BusinessAdmin), "Stock received.");
        foreach (var serial in new string?[] { null, "", " ", "A\0", Repeat("😀", 101) })
        {
            Error(await Damage(id, serial, "DAMAGED", _app.Staff), 400, "BAD_REQUEST", "SerialNumber");
        }
        Assert.Equal((1, (int?)0), await Stock(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-05 DoesNotCreateAMissingStockRecord")]
    public async Task DoesNotCreateAMissingStockRecord()
    {
        var none = Guid.NewGuid();
        Error(await Damage(none, "D", "DAMAGED", _app.Staff), 404, "NOT_FOUND", message: "No inventory record was found for this product.");
        Assert.True(await Stock(none) == null, "missing stock stays missing");
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-06 MatchesTheProductIdAndSerialTogether")]
    public async Task MatchesTheProductIdAndSerialTogether()
    {
        var one = Guid.NewGuid();
        Ok(await Receive(one, ["R1", "R2"], _app.BusinessAdmin), "Stock received.");
        var id = Guid.NewGuid();
        await SeedStock(id, 4, 0);
        Error(await Damage(id, "R1", "DAMAGED", _app.Staff), 404, "NOT_FOUND", message: "No product unit was found for this serial number.");
        Assert.Equal("IN_STOCK", await Status(one, "R1"));
        Assert.Equal((4, (int?)0), await Stock(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-07 RejectsCurrentStatusesOtherThanInStock")]
    public async Task RejectsCurrentStatusesOtherThanInStock()
    {
        foreach (string current in new[] { "RESERVED", "SOLD", "DAMAGED" })
        {
            var id = Guid.NewGuid();
            await SeedStock(id, 4, 1);
            await SeedUnit(id, "D", current);
            Error(await Damage(id, "D", "DAMAGED", _app.Staff), 422, "INVENTORY.INVALID_PRODUCT_UNIT_TRANSITION");
            Assert.Equal(current, await Status(id, "D"));
            Assert.Equal((4, (int?)1), await Stock(id));
        }
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-08 LeavesTheUnitInStockWhenNoQuantityIsAvailable")]
    public async Task LeavesTheUnitInStockWhenNoQuantityIsAvailable()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, 0, 1);
        await SeedUnit(id, "D", "IN_STOCK");
        Error(await Damage(id, "D", "DAMAGED", _app.Staff), 422, "INVENTORY.INSUFFICIENT_AVAILABLE_STOCK");
        Assert.Equal((0, (int?)1), await Stock(id));
        Assert.Equal("IN_STOCK", await Status(id, "D"));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-09 ConcurrentDamageRequestsDecrementStockOnce")]
    public async Task ConcurrentDamageRequestsDecrementStockOnce()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, 2, 0);
        await SeedUnit(id, "D", "IN_STOCK");
        var both = await Task.WhenAll(Damage(id, "D", "DAMAGED", _app.Staff), Damage(id, "D", "DAMAGED", _app.Staff));
        Ok(Assert.Single(both, r => r.Code == 200), "Product unit status updated.");
        Error(Assert.Single(both, r => r.Code == 422), 422, "INVENTORY.INVALID_PRODUCT_UNIT_TRANSITION");
        Assert.Equal((1, (int?)0), await Stock(id));
        Assert.Equal("DAMAGED", await Status(id, "D"));
        Error(await Damage(id, "D", "DAMAGED", _app.Staff), 422, "INVENTORY.INVALID_PRODUCT_UNIT_TRANSITION");
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-10 RollsBackTheUnitUpdateWhenTheStockUpdateFails")]
    public async Task RollsBackTheUnitUpdateWhenTheStockUpdateFails()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, 2, 1);
        await SeedUnit(id, "D", "IN_STOCK");
        await ExecuteSqlAsync($"""ALTER TABLE inventory."Invetory" ADD CONSTRAINT fixture_reject_damage CHECK ("ProductId" <> '{id}'::uuid OR "Quantity" <> 1)""", []);
        try
        {
            Error(await Damage(id, "D", "DAMAGED", _app.Staff), 500, "INTERNAL_SERVER_ERROR");
            Assert.Equal((2, (int?)1), await Stock(id));
            Assert.Equal("IN_STOCK", await Status(id, "D"));
        }
        finally
        {
            await ExecuteSqlAsync("""ALTER TABLE inventory."Invetory" DROP CONSTRAINT fixture_reject_damage""", []);
        }
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-11 RejectsUnauthorizedAndForbiddenStatusChanges")]
    public async Task RejectsUnauthorizedAndForbiddenStatusChanges()
    {
        var one = Guid.NewGuid();
        Ok(await Receive(one, ["R1", "R2"], _app.BusinessAdmin), "Stock received.");
        await AssertRestrictedAsync(credential => Damage(one, "R1", "DAMAGED", credential));
        Assert.Equal("IN_STOCK", await Status(one, "R1"));
        Assert.Equal((2, (int?)0), await Stock(one));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-12 HandlesInvalidRoutesAndRequestBodies")]
    public async Task HandlesInvalidRoutesAndRequestBodies()
    {
        var one = Guid.NewGuid();
        Ok(await Receive(one, ["R1", "R2"], _app.BusinessAdmin), "Stock received.");
        Error(await Damage(Guid.Empty, "D", "DAMAGED", _app.Staff), 400, "BAD_REQUEST", "ProductId");
        Assert.Equal(404, (await _app.SendAsync(HttpMethod.Patch, "/api/inventory/not-a-uuid/units/status", _app.Staff, new { serialNumber = "D", status = "DAMAGED" })).Code);
        Error(await _app.SendAsync(HttpMethod.Patch, DamagePath(one), _app.Staff, new StringContent("{", Encoding.UTF8, "application/json")), 400, "BAD_REQUEST");
        Assert.Equal(415, (await _app.SendAsync(HttpMethod.Patch, DamagePath(one), _app.Staff, new StringContent("plain", Encoding.UTF8, "text/plain"))).Code);
        Assert.Equal((2, (int?)0), await Stock(one));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-13 LooksUpTheOriginalSerialAndPreservesNullReservations")]
    public async Task LooksUpTheOriginalSerialAndPreservesNullReservations()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, 2, null);
        await SeedUnit(id, "  MiXeD  ", "IN_STOCK");
        Error(await Damage(id, "mixed", "DAMAGED", _app.Staff), 404, "NOT_FOUND");
        Ok(await Damage(id, "  MiXeD  ", "DAMAGED", _app.Staff), "Product unit status updated.");
        Assert.Equal("DAMAGED", await Status(id, "  MiXeD  "));
        Assert.Equal((1, (int?)null), await Stock(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-STA-14 RejectsStockWithANegativeReservedQuantity")]
    public async Task RejectsStockWithANegativeReservedQuantity()
    {
        var id = Guid.NewGuid();
        await SeedStock(id, 5, -1);
        await SeedUnit(id, "D", "IN_STOCK");
        Error(await Damage(id, "D", "DAMAGED", _app.Staff), 422, "INVENTORY.INVALID_STOCK_QUANTITY");
        Assert.Equal((5, (int?)-1), await Stock(id));
        Assert.Equal("IN_STOCK", await Status(id, "D"));
    }
}
