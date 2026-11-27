using MediatR;

namespace InventoryModule.Application.UseCases.ReceiveStock;

internal sealed record ReceiveStock_UCInput(Guid ProductId, List<string> SerialNumbers)
    : IRequest<ReceiveStock_UCOutput>;
