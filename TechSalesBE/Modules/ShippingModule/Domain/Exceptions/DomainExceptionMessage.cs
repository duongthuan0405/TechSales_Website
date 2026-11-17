namespace ShippingModule.Domain.Exceptions;

public static class DomainExceptionMessage
{
    public const string AddressDeleted = "Địa chỉ giao hàng đã bị xóa.";
    public const string AddressIdentityInvalid = "Thông tin định danh địa chỉ không hợp lệ.";
    public const string AddressInvalid = "Thông tin địa chỉ giao hàng không hợp lệ.";
    public const string AddressNotFound = "Không tìm thấy địa chỉ giao hàng.";
    public const string AddressNotOwned = "Bạn không được phép quản lý địa chỉ của người khác.";
    public const string DeliveryUnsupported = "Hiện chưa hỗ trợ giao hàng đến địa phương này.";
    public const string ShippingFeeInvalid = "Cấu hình phí vận chuyển không hợp lệ.";
}
