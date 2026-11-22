using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;

namespace CatalogModule.Application.UseCases.GetBrandById;

internal sealed class GetBrandById_UCHandler : IRequestHandler<GetBrandById_UCInput, GetBrandById_UCOutput>
{
    private readonly IBrandRepository _brands;

    public GetBrandById_UCHandler(IBrandRepository brands)
    {
        _brands = brands;
    }

    public async Task<GetBrandById_UCOutput> Handle(GetBrandById_UCInput input, CancellationToken cancellationToken)
    {
        Brand? brand = await _brands.GetByIdAsync(input.Id, cancellationToken);
        if (brand == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.BrandNotFound);
        }

        return new GetBrandById_UCOutput(brand.Id, brand.Name);
    }
}
