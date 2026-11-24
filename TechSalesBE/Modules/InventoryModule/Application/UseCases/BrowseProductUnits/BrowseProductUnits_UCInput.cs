using MediatR;

namespace InventoryModule.Application.UseCases.BrowseProductUnits;

public sealed record BrowseProductUnits_UCInput(Guid? ProductId, string? Keyword, string? Status,
    int PageNumber, int PageSize) : IRequest<BrowseProductUnits_UCOutput>;
