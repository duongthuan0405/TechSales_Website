namespace CatalogModule.Application.UseCases.BrowseBrandsForManagement;

public sealed record BrowseBrandsForManagement_UCOutput(List<BrowseBrandsForManagement_UCOutput.Item> Items,
    int TotalCount, int PageNumber, int PageSize)
{
    public sealed record Item(Guid Id, string Name);
}
