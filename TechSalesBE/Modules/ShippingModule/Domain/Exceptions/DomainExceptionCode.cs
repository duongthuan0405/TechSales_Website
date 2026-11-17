namespace ShippingModule.Domain.Exceptions;

internal static class DomainExceptionCode
{
    public const string DELIVERY_NOT_SUPPORTED = "SHIPPING.DELIVERY_NOT_SUPPORTED";
    public const string INVALID_SHIPPING_FEE = "SHIPPING.INVALID_SHIPPING_FEE";
    public const string INVALID_ADDRESS = "SHIPPING.INVALID_ADDRESS";
    public const string ADDRESS_DELETED = "SHIPPING.ADDRESS_DELETED";
    public const string RULE_ADDRESS_NOT_FOUND = "SHIPPING.RULE_ADDRESS_NOT_FOUND";
    public const string RULE_ADDRESS_NOT_OWNED = "SHIPPING.RULE_ADDRESS_NOT_OWNED";
}
