namespace CatalogModule.Application.UseCases.GetAllBrand;

internal sealed record GetAllBrand_UCOutput(List<GetAllBrand_UCOutput.BrandItem> Items)
{
    internal sealed record BrandItem(Guid Id, string Name);
}
