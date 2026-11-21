using InventoryModule.Domain.Exceptions;

namespace InventoryModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string InvalidProductId = DomainExceptionMessage.InvalidProductId;
    public static readonly string InvalidSerialNumber = DomainExceptionMessage.InvalidSerialNumber;
    public const string SerialNumbersRequired = "Specify at least one serial number.";
    public const string DuplicateSerialNumbers = "Serial numbers must not be duplicated.";
    public const string SerialNumberAlreadyExists = "One or more serial numbers are already registered for this product.";
}
