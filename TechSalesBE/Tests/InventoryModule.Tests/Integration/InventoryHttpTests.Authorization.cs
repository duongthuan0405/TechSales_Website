using Xunit;
using static InventoryModule.Tests.Integration.InventoryWorkbook;

namespace InventoryModule.Tests.Integration;

public sealed partial class InventoryHttpTests
{
    [InventoryPostgresFact(DisplayName = "INV-AUTH-01 RejectsTokensAfterTheUserBecomesInactive")]
    public async Task RejectsTokensAfterTheUserBecomesInactive()
    {
        (Guid userId, string token) = await _app.CreateAccountAsync("Staff");
        var productId = Guid.NewGuid();
        var missingProductId = Guid.NewGuid();
        Ok(await Receive(productId, ["D"], _app.BusinessAdmin), "Stock received.");
        await _app.InactivateAccountAsync(userId);

        Error(await Receive(missingProductId, ["R"], token), 401, "UNAUTHORIZED");
        Error(await Damage(productId, "D", "DAMAGED", token), 401, "UNAUTHORIZED");
        Error(await Import(Workbook(DataRow(2, missingProductId, "X")), token), 401, "UNAUTHORIZED");
        Assert.Null(await Stock(missingProductId));
        Assert.Equal((1, (int?)0), await Stock(productId));
        Assert.Equal("IN_STOCK", await Status(productId, "D"));
    }

    [InventoryPostgresFact]
    public async Task OnlyAdministratorsCanReceiveAndImportStock()
    {
        var rejectedProduct = Guid.NewGuid();
        Error(await Receive(rejectedProduct, ["R"], _app.Staff), 403, "FORBIDDEN");
        Error(await Import(Workbook(DataRow(2, rejectedProduct, "X")), _app.Staff), 403, "FORBIDDEN");
        Assert.Null(await Stock(rejectedProduct));
        foreach (string token in new[] { _app.BusinessAdmin, _app.TechnicalAdmin })
        {
            var productId = Guid.NewGuid();
            Ok(await Receive(productId, ["R"], token), "Stock received.");
            Ok(await Damage(productId, "R", "DAMAGED", token), "Product unit status updated.");
            Ok(await Import(Workbook(DataRow(2, productId, "X")), token), "Stock imported.");
            Assert.Equal((1, (int?)0), await Stock(productId));
            Assert.Equal("DAMAGED", await Status(productId, "R"));
            Assert.Equal("IN_STOCK", await Status(productId, "X"));
        }
    }
}
