using CatalogModule.Application.Constants;
using MediatR;

namespace CatalogModule.Application.UseCases.BrowseCategoriesForManagement;

internal sealed record BrowseCategoriesForManagement_UCInput(int PageNumber = 1,
    int PageSize = CatalogPagination.DefaultPageSize, string? Keyword = null) : IRequest<BrowseCategoriesForManagement_UCOutput>;
