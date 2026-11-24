using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;

namespace CatalogModule.Application.UseCases.GetCategoryById;

internal sealed class GetCategoryById_UCHandler : IRequestHandler<GetCategoryById_UCInput, GetCategoryById_UCOutput>
{
    private readonly ICategoryRepository _categories;

    public GetCategoryById_UCHandler(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<GetCategoryById_UCOutput> Handle(
        GetCategoryById_UCInput input,
        CancellationToken cancellationToken)
    {
        Category? category = await _categories.GetByIdAsync(input.Id, cancellationToken);
        if (category == null)
        {
            throw new BusinessException(
                BusinessExceptionCode.NOT_FOUND,
                BusinessExceptionMessage.CategoryNotFound);
        }

        return new GetCategoryById_UCOutput(category.Id, category.Name);
    }
}
