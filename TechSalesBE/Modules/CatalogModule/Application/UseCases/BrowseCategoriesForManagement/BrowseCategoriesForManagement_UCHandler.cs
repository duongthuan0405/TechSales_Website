using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using MediatR;

namespace CatalogModule.Application.UseCases.BrowseCategoriesForManagement;

internal sealed class BrowseCategoriesForManagement_UCHandler : IRequestHandler<BrowseCategoriesForManagement_UCInput, BrowseCategoriesForManagement_UCOutput>
{
    private readonly ICategoryRepository _repository;

    public BrowseCategoriesForManagement_UCHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<BrowseCategoriesForManagement_UCOutput> Handle(BrowseCategoriesForManagement_UCInput input, CancellationToken cancellationToken)
    {
        (List<Category> rows, int totalCount) = await _repository.GetPageAsync(
            input.PageNumber, input.PageSize, input.Keyword, cancellationToken);
        List<BrowseCategoriesForManagement_UCOutput.Item> items = rows.Select(row => new BrowseCategoriesForManagement_UCOutput.Item(row.Id, row.Name)).ToList();
        return new BrowseCategoriesForManagement_UCOutput(items, totalCount, input.PageNumber, input.PageSize);
    }
}
