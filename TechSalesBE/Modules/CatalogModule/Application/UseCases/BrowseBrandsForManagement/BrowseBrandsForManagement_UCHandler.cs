using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using MediatR;

namespace CatalogModule.Application.UseCases.BrowseBrandsForManagement;

internal sealed class BrowseBrandsForManagement_UCHandler : IRequestHandler<BrowseBrandsForManagement_UCInput, BrowseBrandsForManagement_UCOutput>
{
    private readonly IBrandRepository _repository;

    public BrowseBrandsForManagement_UCHandler(IBrandRepository repository)
    {
        _repository = repository;
    }

    public async Task<BrowseBrandsForManagement_UCOutput> Handle(BrowseBrandsForManagement_UCInput input, CancellationToken cancellationToken)
    {
        (List<Brand> rows, int totalCount) = await _repository.GetPageAsync(
            input.PageNumber, input.PageSize, input.Keyword, cancellationToken);
        List<BrowseBrandsForManagement_UCOutput.Item> items = rows.Select(row => new BrowseBrandsForManagement_UCOutput.Item(row.Id, row.Name)).ToList();
        return new BrowseBrandsForManagement_UCOutput(items, totalCount, input.PageNumber, input.PageSize);
    }
}
