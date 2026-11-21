using CommonModule.Domain.Exceptions;
using InventoryModule.Domain.Exceptions;

namespace InventoryModule.Domain.Rules;

internal static class InventoryRules
{
    public static void EnsureProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_ID, DomainExceptionMessage.InvalidProductId);
        }
    }

    public static void EnsureStockQuantities(int quantity, int? reservedQuantity)
    {
        if (quantity < 0 || reservedQuantity < 0)
        {
            throw new DomainException(
                DomainExceptionCode.INVALID_STOCK_QUANTITY, DomainExceptionMessage.InvalidStockQuantity);
        }
    }

    public static void EnsurePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException(
                DomainExceptionCode.INVALID_OPERATION_QUANTITY, DomainExceptionMessage.InvalidOperationQuantity);
        }
    }

    public static void EnsureAvailableStock(int availableQuantity, int quantity)
    {
        if (quantity > availableQuantity)
        {
            throw new DomainException(
                DomainExceptionCode.INSUFFICIENT_AVAILABLE_STOCK, DomainExceptionMessage.InsufficientAvailableStock);
        }
    }

    public static void EnsureReservedStock(int reservedQuantity, int quantity)
    {
        if (quantity > reservedQuantity)
        {
            throw new DomainException(
                DomainExceptionCode.INSUFFICIENT_RESERVED_STOCK, DomainExceptionMessage.InsufficientReservedStock);
        }
    }

    public static void EnsureCapacity(int currentQuantity, int quantity)
    {
        if (quantity > int.MaxValue - currentQuantity)
        {
            throw new DomainException(
                DomainExceptionCode.STOCK_QUANTITY_LIMIT_EXCEEDED, DomainExceptionMessage.StockQuantityLimitExceeded);
        }
    }
}
