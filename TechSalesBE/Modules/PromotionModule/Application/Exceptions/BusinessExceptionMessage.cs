using PromotionModule.Domain.Constants;

namespace PromotionModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string VoucherNotFound = "Không tìm thấy voucher.";
    public const string DuplicateCode = "Mã voucher đã tồn tại.";
    public static string InvalidCode => $"Mã voucher không được để trống và không vượt quá {PromotionLimits.VoucherCodeMaxLength} ký tự.";
    public const string InvalidType = "Loại voucher phải là FIXED hoặc PERCENT.";
    public static string InvalidValue => $"Giá trị giảm phải từ {PromotionLimits.MinimumDiscountValue} trở lên; voucher phần trăm không được vượt quá {PromotionLimits.MaximumPercent}%.";
    public static string InvalidMaximumUsage => $"Giới hạn lượt dùng phải từ {PromotionLimits.MinimumUsageLimit} trở lên nếu được cấu hình.";
    public const string UsageLimitTooSmall = "Giới hạn lượt dùng không được nhỏ hơn số lượt đã sử dụng.";
    public static string InvalidMinimumOrder => $"Giá trị đơn hàng tối thiểu phải từ {PromotionLimits.MinimumOrderAmount} trở lên.";
    public const string InvalidDateRange = "Ngày kết thúc phải sau ngày bắt đầu.";
    public const string InvalidPage = "Thông tin phân trang không hợp lệ.";
}
