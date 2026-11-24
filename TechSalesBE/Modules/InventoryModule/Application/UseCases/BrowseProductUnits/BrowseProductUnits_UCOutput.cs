namespace InventoryModule.Application.UseCases.BrowseProductUnits;

public sealed record BrowseProductUnits_UCOutput(List<BrowseProductUnits_UCOutput.Item> Items,
    int TotalCount, int PageNumber, int PageSize)
{
    public sealed record Item(Guid ProductId, string SerialNumber, string Status);
}
