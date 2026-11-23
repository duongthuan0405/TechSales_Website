using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using MediatR;

namespace CatalogModule.Application.UseCases.GetAllCategory;

internal sealed class GetAllCategory_UCHandler : IRequestHandler<GetAllCategory_UCInput, GetAllCategory_UCOutput>
{
    private readonly ICategoryRepository _categories;

    public GetAllCategory_UCHandler(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<GetAllCategory_UCOutput> Handle(
        GetAllCategory_UCInput input,
        CancellationToken cancellationToken)
    {
        List<Category> categories = await _categories.GetAllAsync(cancellationToken);
        List<GetAllCategory_UCOutput.CategoryItem> items = new List<GetAllCategory_UCOutput.CategoryItem>();

        foreach (Category category in categories)
        {
            items.Add(new GetAllCategory_UCOutput.CategoryItem(category.Id, category.Name));
        }

        return new GetAllCategory_UCOutput(items);
    }
}
