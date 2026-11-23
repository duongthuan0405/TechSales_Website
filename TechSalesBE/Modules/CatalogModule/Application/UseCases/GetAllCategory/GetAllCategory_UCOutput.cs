namespace CatalogModule.Application.UseCases.GetAllCategory;

internal sealed record GetAllCategory_UCOutput(List<GetAllCategory_UCOutput.CategoryItem> Items)
{
    internal sealed record CategoryItem(Guid Id, string Name);
}
