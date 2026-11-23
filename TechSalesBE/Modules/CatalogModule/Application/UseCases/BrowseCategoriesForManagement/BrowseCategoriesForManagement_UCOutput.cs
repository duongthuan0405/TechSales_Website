namespace CatalogModule.Application.UseCases.BrowseCategoriesForManagement;

public sealed record BrowseCategoriesForManagement_UCOutput(List<BrowseCategoriesForManagement_UCOutput.Item> Items,
    int TotalCount, int PageNumber, int PageSize)
{
    public sealed record Item(Guid Id, string Name);
}
