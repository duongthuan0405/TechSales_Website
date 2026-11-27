using InventoryModule.Application.UseCases.BrowseProductUnits;
using Xunit;

namespace InventoryModule.Tests;

public sealed class BrowseProductUnitsTests
{
    [Theory]
    [InlineData("IN_STOCK")]
    [InlineData("RESERVED")]
    [InlineData("SOLD")]
    [InlineData("DAMAGED")]
    [InlineData(null)]
    public void AcceptsKnownStatusesAndOptionalProduct(string? status)
    {
        var validator = new BrowseProductUnits_UCInput_Validator();
        Assert.True(validator.Validate(new BrowseProductUnits_UCInput(null, "SN", status, 1, 20)).IsValid);
    }

    [Theory]
    [InlineData("0", 1, 20)]
    [InlineData("NOT_A_STATUS", 1, 20)]
    [InlineData("DAMAGED", 0, 20)]
    [InlineData("DAMAGED", 1, 101)]
    public void RejectsInvalidFiltersAndUnboundedPages(string status, int page, int size)
    {
        var validator = new BrowseProductUnits_UCInput_Validator();
        Assert.False(validator.Validate(new BrowseProductUnits_UCInput(null, null, status, page, size)).IsValid);
    }
}
