using InventoryModule.Application.Repositories;
using MediatR;

namespace InventoryModule.Application.UseCases.BrowseProductUnits;

internal sealed class BrowseProductUnits_UCHandler : IRequestHandler<BrowseProductUnits_UCInput, BrowseProductUnits_UCOutput>
{
    private readonly IProductUnitReader _reader;

    public BrowseProductUnits_UCHandler(IProductUnitReader reader)
    {
        _reader = reader;
    }

    public async Task<BrowseProductUnits_UCOutput> Handle(BrowseProductUnits_UCInput input, CancellationToken cancellationToken)
    {
        var page = await _reader.BrowseAsync(input.ProductId, input.Keyword, input.Status,
            input.PageNumber, input.PageSize, cancellationToken);
        List<BrowseProductUnits_UCOutput.Item> items = page.Items.Select(unit =>
            new BrowseProductUnits_UCOutput.Item(unit.ProductId, unit.SerialNumber, unit.Status.ToString())).ToList();
        return new BrowseProductUnits_UCOutput(items, page.TotalCount, input.PageNumber, input.PageSize);
    }
}
