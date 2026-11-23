namespace CartModule.Domain.Exceptions;

internal static class DomainExceptionCode
{
    public const string ProductUnavailable = "CART.PRODUCT_UNAVAILABLE";
    public const string InvalidId = "CART_INVALID_ID";
    public const string InvalidQuantity = "CART_INVALID_QUANTITY";
    public const string QuantityOverflow = "CART_QUANTITY_OVERFLOW";
    public const string InvalidTimestamp = "CART_INVALID_TIMESTAMP";
    public const string ItemCartMismatch = "CART_ITEM_CART_MISMATCH";
    public const string DuplicateProduct = "CART_DUPLICATE_PRODUCT";
    public const string ItemNotFound = "CART_ITEM_NOT_FOUND";
}
