using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Domain.Entities;
using MediatR;

namespace CatalogModule.Application.UseCases.CreateCategory;

internal sealed class CreateCategory_UCHandler : IRequestHandler<CreateCategory_UCInput, CreateCategory_UCOutput>
{
    private readonly ICategoryRepository _categories;
    private readonly IExecuteAtomically _transaction;

    public CreateCategory_UCHandler(ICategoryRepository categories, IExecuteAtomically transaction)
    {
        _categories = categories;
        _transaction = transaction;
    }

    public async Task<CreateCategory_UCOutput> Handle(
        CreateCategory_UCInput input,
        CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Category category = Category.Create(input.Name);
            await _categories.SaveAsync(category, cancellationToken);
            return new CreateCategory_UCOutput(category.Id, category.Name);
        }, cancellationToken);
    }
}
