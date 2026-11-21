using InventoryModule.Domain.Constants;

namespace InventoryModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public const string InvalidProductId = "The product ID is invalid.";
    public const string InvalidStockQuantity = "Available and reserved stock quantities must be zero or greater.";
    public const string InvalidOperationQuantity = "The quantity must be greater than zero.";
    public const string InsufficientAvailableStock = "There is not enough available stock.";
    public const string InsufficientReservedStock = "There is not enough reserved stock.";
    public const string StockQuantityLimitExceeded = "The stock quantity exceeds the allowed limit.";
    public static readonly string InvalidSerialNumber =
        $"Enter a serial number with at most {ProductUnitConstants.MaxSerialNumberLength} characters.";
    public const string InvalidProductUnitStatus = "The product unit status is invalid.";
    public const string InvalidProductUnitTransition =
        "The current status of the product unit does not allow this operation.";
}
