namespace CartModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public const string InvalidId = "{0} must not be empty.";
    public const string InvalidQuantity = "Cart item quantity must be greater than zero.";
    public const string QuantityOverflow = "Cart item quantity exceeds the supported integer range.";
    public const string InvalidTimestamp = "The cart item update time must be at or after the previous timestamp.";
    public const string ItemCartMismatch = "The cart item belongs to a different cart.";
    public const string DuplicateProduct = "The cart already includes an item for this product.";
    public const string ItemNotFound = "The product was not found in the cart.";
}
