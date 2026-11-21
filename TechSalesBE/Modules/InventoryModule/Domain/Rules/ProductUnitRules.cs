using CommonModule.Domain.Exceptions;
using InventoryModule.Domain.Constants;
using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Exceptions;

namespace InventoryModule.Domain.Rules;

internal static class ProductUnitRules
{
    public static void EnsureSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber) ||
            serialNumber.EnumerateRunes().Count() > ProductUnitConstants.MaxSerialNumberLength)
        {
            throw new DomainException(
                DomainExceptionCode.INVALID_SERIAL_NUMBER, DomainExceptionMessage.InvalidSerialNumber);
        }
    }

    public static void EnsureStatus(ProductUnitStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException(
                DomainExceptionCode.INVALID_PRODUCT_UNIT_STATUS, DomainExceptionMessage.InvalidProductUnitStatus);
        }
    }

    public static void EnsureTransition(ProductUnitStatus currentStatus, ProductUnitStatus targetStatus)
    {
        bool allowed = (currentStatus, targetStatus) switch
        {
            (ProductUnitStatus.IN_STOCK, ProductUnitStatus.RESERVED or ProductUnitStatus.DAMAGED) => true,
            (ProductUnitStatus.RESERVED, ProductUnitStatus.IN_STOCK or ProductUnitStatus.SOLD) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new DomainException(
                DomainExceptionCode.INVALID_PRODUCT_UNIT_TRANSITION, DomainExceptionMessage.InvalidProductUnitTransition);
        }
    }
}
