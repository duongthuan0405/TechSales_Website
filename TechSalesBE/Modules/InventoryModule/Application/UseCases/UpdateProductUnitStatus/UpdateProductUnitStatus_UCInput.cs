using MediatR;

namespace InventoryModule.Application.UseCases.UpdateProductUnitStatus;

internal sealed record UpdateProductUnitStatus_UCInput(Guid ProductId, string SerialNumber, string Status)
    : IRequest<UpdateProductUnitStatus_UCOutput>;
