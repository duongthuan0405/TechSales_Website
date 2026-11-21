using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;

namespace CatalogModule.Application.UseCases.CreateBrand;

internal sealed class CreateBrand_UCHandler : IRequestHandler<CreateBrand_UCInput, CreateBrand_UCOutput>
{
    private readonly IBrandRepository _brands;
    private readonly IExecuteAtomically _transaction;

    public CreateBrand_UCHandler(IBrandRepository brands, IExecuteAtomically transaction)
    {
        _brands = brands;
        _transaction = transaction;
    }

    public async Task<CreateBrand_UCOutput> Handle(CreateBrand_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Brand brand = Brand.Create(input.Name);
            Brand? existingBrand = await _brands.GetByNameAsync(brand.Name, cancellationToken);
            if (existingBrand != null)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.DuplicateBrandName);
            }

            await _brands.SaveAsync(brand, cancellationToken);
            return new CreateBrand_UCOutput(brand.Id, brand.Name);
        }, cancellationToken);
    }
}
