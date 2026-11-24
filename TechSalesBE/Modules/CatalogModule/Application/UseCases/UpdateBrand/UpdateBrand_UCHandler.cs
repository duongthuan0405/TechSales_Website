using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;

namespace CatalogModule.Application.UseCases.UpdateBrand;

internal sealed class UpdateBrand_UCHandler : IRequestHandler<UpdateBrand_UCInput, UpdateBrand_UCOutput>
{
    private readonly IBrandRepository _brands;
    private readonly IExecuteAtomically _transaction;

    public UpdateBrand_UCHandler(IBrandRepository brands, IExecuteAtomically transaction)
    {
        _brands = brands;
        _transaction = transaction;
    }

    public async Task<UpdateBrand_UCOutput> Handle(UpdateBrand_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Brand? brand = await _brands.GetByIdAsync(input.Id, cancellationToken);
            if (brand == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.BrandNotFound);
            }

            Brand? existingBrand = await _brands.GetByNameAsync(input.Name!, cancellationToken);
            if (existingBrand != null && existingBrand.Id != brand.Id)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.DuplicateBrandName);
            }

            brand.Update(input.Name);
            await _brands.SaveAsync(brand, cancellationToken);
            return new UpdateBrand_UCOutput(brand.Id, brand.Name);
        }, cancellationToken);
    }
}
