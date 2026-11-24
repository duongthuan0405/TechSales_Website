using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using MediatR;

namespace CatalogModule.Application.UseCases.GetAllBrand;

internal sealed class GetAllBrand_UCHandler : IRequestHandler<GetAllBrand_UCInput, GetAllBrand_UCOutput>
{
    private readonly IBrandRepository _brands;

    public GetAllBrand_UCHandler(IBrandRepository brands)
    {
        _brands = brands;
    }

    public async Task<GetAllBrand_UCOutput> Handle(GetAllBrand_UCInput input, CancellationToken cancellationToken)
    {
        List<Brand> brands = await _brands.GetAllAsync(cancellationToken);
        List<GetAllBrand_UCOutput.BrandItem> items = new List<GetAllBrand_UCOutput.BrandItem>();
        foreach (Brand brand in brands)
        {
            items.Add(new GetAllBrand_UCOutput.BrandItem(brand.Id, brand.Name));
        }

        return new GetAllBrand_UCOutput(items);
    }
}
