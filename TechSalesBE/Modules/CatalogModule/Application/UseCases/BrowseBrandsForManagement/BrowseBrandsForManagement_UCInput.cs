using CatalogModule.Application.Constants;
using MediatR;

namespace CatalogModule.Application.UseCases.BrowseBrandsForManagement;

internal sealed record BrowseBrandsForManagement_UCInput(int PageNumber = 1,
    int PageSize = CatalogPagination.DefaultPageSize, string? Keyword = null) : IRequest<BrowseBrandsForManagement_UCOutput>;
