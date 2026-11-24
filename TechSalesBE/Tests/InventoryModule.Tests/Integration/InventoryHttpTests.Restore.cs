using Xunit;

namespace InventoryModule.Tests.Integration;

public sealed partial class InventoryHttpTests
{
    [InventoryPostgresFact]
    public async Task ConcurrentRestorationsIncrementStockOnlyOnce()
    {
        Guid product = Guid.NewGuid();
        Ok(await Receive(product, ["RESTORE"], _app.BusinessAdmin), "Stock received.");
        Ok(await Damage(product, "RESTORE", "DAMAGED", _app.Staff), "Product unit status updated.");
        var responses = await Task.WhenAll(
            Damage(product, "RESTORE", "IN_STOCK", _app.Staff),
            Damage(product, "RESTORE", "IN_STOCK", _app.Staff));
        Assert.Equal(1, responses.Count(response => response.Code == 200));
        Assert.Equal(1, responses.Count(response => response.Code == 422));
        Assert.Equal((1, (int?)0), await Stock(product));
        Assert.Equal("IN_STOCK", await Status(product, "RESTORE"));
    }
}
