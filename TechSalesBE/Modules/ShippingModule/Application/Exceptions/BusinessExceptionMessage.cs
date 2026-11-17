using ShippingModule.Domain.Constants;

namespace ShippingModule.Application.Exceptions;

public static class BusinessExceptionMessage
{
    public static string AddressDetailInvalid => $"Địa chỉ chi tiết không được để trống và không vượt quá {ShippingLimits.AddressDetailMaxLength} ký tự.";
    public const string AddressNotFound = global::ShippingModule.Domain.Exceptions.DomainExceptionMessage.AddressNotFound;
    public const string AddressNotOwned = global::ShippingModule.Domain.Exceptions.DomainExceptionMessage.AddressNotOwned;
    public const string LocationSelectionInvalid = "Vui lòng cung cấp mã tỉnh hoặc mã phường, không cung cấp đồng thời cả hai.";
    public const string NegativeShippingFee = "Phí vận chuyển không được âm.";
    public const string ProvinceCodeInvalid = "Mã tỉnh hoặc thành phố không hợp lệ.";
    public const string ProvinceNotFound = "Không tìm thấy tỉnh hoặc thành phố.";
    public const string WardCodeInvalid = "Mã phường hoặc xã không hợp lệ.";
    public const string WardNotFound = "Không tìm thấy phường hoặc xã.";
}
