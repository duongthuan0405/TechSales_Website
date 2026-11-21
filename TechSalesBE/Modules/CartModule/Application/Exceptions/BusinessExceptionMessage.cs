namespace CartModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string InvalidUserId = "A non-empty user ID is required to access the cart.";
    public const string InvalidProductId = "Product ID must be a non-empty UUID.";
    public const string InvalidQuantityToAdd = "Quantity to add must be a positive integer.";
}
