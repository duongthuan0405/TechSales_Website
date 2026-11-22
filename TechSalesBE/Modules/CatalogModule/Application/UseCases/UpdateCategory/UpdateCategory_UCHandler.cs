using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;

namespace CatalogModule.Application.UseCases.UpdateCategory;

internal sealed class UpdateCategory_UCHandler : IRequestHandler<UpdateCategory_UCInput, UpdateCategory_UCOutput>
{
    private readonly ICategoryRepository _categories;
    private readonly IExecuteAtomically _transaction;

    public UpdateCategory_UCHandler(ICategoryRepository categories, IExecuteAtomically transaction)
    {
        _categories = categories;
        _transaction = transaction;
    }

    public async Task<UpdateCategory_UCOutput> Handle(
        UpdateCategory_UCInput input,
        CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Category? category = await _categories.GetByIdAsync(input.Id, cancellationToken);
            if (category == null)
            {
                throw new BusinessException(
                    BusinessExceptionCode.NOT_FOUND,
                    BusinessExceptionMessage.CategoryNotFound);
            }

            category.Update(input.Name);
            await _categories.SaveAsync(category, cancellationToken);
            return new UpdateCategory_UCOutput(category.Id, category.Name);
        }, cancellationToken);
    }
}
