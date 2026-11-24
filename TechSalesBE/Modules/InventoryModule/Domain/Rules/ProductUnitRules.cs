using CommonModule.Domain.Exceptions;
using InventoryModule.Domain.Constants;
using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Exceptions;

namespace InventoryModule.Domain.Rules;

internal static class ProductUnitRules
{
    public static bool IsBrowseStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            || (Enum.TryParse(status, out ProductUnitStatus parsed) && Enum.IsDefined(parsed)
                && parsed.ToString() == status);
    }

    public static bool IsValidSerialNumber(string? serialNumber)
    {
        return !string.IsNullOrWhiteSpace(serialNumber)
            && !serialNumber.Contains('\0')
            && serialNumber.EnumerateRunes().Count() <= ProductUnitConstants.MaxSerialNumberLength;
    }

    public static bool HasDistinctSerialNumbers(IReadOnlyCollection<string> serialNumbers)
    {
        return serialNumbers.Distinct(StringComparer.Ordinal).Count() == serialNumbers.Count;
    }

    public static bool IsManualStatusTarget(string? status)
    {
        return string.Equals(status, nameof(ProductUnitStatus.DAMAGED), StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, nameof(ProductUnitStatus.IN_STOCK), StringComparison.OrdinalIgnoreCase);
    }

    public static void EnsureSerialNumber(string serialNumber)
    {
        if (!IsValidSerialNumber(serialNumber))
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

    public static void EnsureManualRestoration(ProductUnitStatus currentStatus)
    {
        if (currentStatus != ProductUnitStatus.DAMAGED)
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_UNIT_TRANSITION,
                DomainExceptionMessage.InvalidProductUnitTransition);
        }
    }

    public static void EnsureTransition(ProductUnitStatus currentStatus, ProductUnitStatus targetStatus)
    {
        bool allowed = (currentStatus, targetStatus) switch
        {
            (ProductUnitStatus.IN_STOCK, ProductUnitStatus.RESERVED or ProductUnitStatus.DAMAGED) => true,
            (ProductUnitStatus.RESERVED, ProductUnitStatus.IN_STOCK or ProductUnitStatus.SOLD) => true,
            (ProductUnitStatus.DAMAGED, ProductUnitStatus.IN_STOCK) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new DomainException(
                DomainExceptionCode.INVALID_PRODUCT_UNIT_TRANSITION, DomainExceptionMessage.InvalidProductUnitTransition);
        }
    }
}
