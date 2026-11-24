using Xunit;

namespace InventoryModule.Tests.Integration;

public sealed partial class InventoryHttpTests
{
    [InventoryPostgresFact]
    public async Task BrowseFiltersStatusSerialAndProductBeforePaging()
    {
        Guid product = Guid.NewGuid();
        Guid otherProduct = Guid.NewGuid();
        Ok(await Receive(product, ["BROWSE-A", "BROWSE-B", "OTHER"], _app.BusinessAdmin), "Stock received.");
        Ok(await Receive(otherProduct, ["BROWSE-A"], _app.BusinessAdmin), "Stock received.");
        Ok(await Damage(product, "BROWSE-B", "DAMAGED", _app.Staff), "Product unit status updated.");
        string path = $"/api/inventory/units?productId={product}&keyword=BROWSE&status=IN_STOCK&pageSize=1";
        foreach (string token in new[] { _app.Staff, _app.BusinessAdmin, _app.TechnicalAdmin })
        {
            var result = await _app.SendAsync(HttpMethod.Get, path, token);
            Ok(result, "Đã tải danh sách thiết bị trong kho.");
            Assert.Equal(1, result.Data.GetProperty("totalCount").GetInt32());
            Assert.Equal("BROWSE-A", result.Data.GetProperty("items")[0].GetProperty("serialNumber").GetString());
        }
        await AssertRestrictedAsync(token => _app.SendAsync(HttpMethod.Get, path, token));
    }
}
