using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Rules;

namespace InventoryModule.Domain.Entities;

internal sealed class ProductUnit
{
    public Guid ProductId { get; }
    public string SerialNumber { get; }
    public ProductUnitStatus Status { get; private set; }

    public ProductUnit(
        Guid productId, string serialNumber, ProductUnitStatus status = ProductUnitStatus.IN_STOCK)
    {
        InventoryRules.EnsureProductId(productId);
        ProductUnitRules.EnsureSerialNumber(serialNumber);
        ProductUnitRules.EnsureStatus(status);

        ProductId = productId;
        SerialNumber = serialNumber;
        Status = status;
    }

    public void Reserve()
    {
        ChangeStatus(ProductUnitStatus.RESERVED);
    }

    public void ReleaseReservation()
    {
        ChangeStatus(ProductUnitStatus.IN_STOCK);
    }

    public void ConfirmSale()
    {
        ChangeStatus(ProductUnitStatus.SOLD);
    }

    public void MarkDamaged()
    {
        ChangeStatus(ProductUnitStatus.DAMAGED);
    }

    private void ChangeStatus(ProductUnitStatus targetStatus)
    {
        ProductUnitRules.EnsureTransition(Status, targetStatus);
        Status = targetStatus;
    }
}
