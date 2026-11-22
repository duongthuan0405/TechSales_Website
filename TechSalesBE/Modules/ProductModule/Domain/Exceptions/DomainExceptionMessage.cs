using ProductModule.Domain.Constants;

namespace ProductModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public static string ProductNameInvalid =>
        $"Tên sản phẩm không được để trống và không được vượt quá {ProductLimits.NameMaxLength} ký tự.";

    public const string ProductDescriptionInvalid = "Mô tả sản phẩm không hợp lệ.";
    public const string ProductBrandInvalid = "Vui lòng chọn thương hiệu cho sản phẩm.";
    public const string ProductCategoryInvalid = "Vui lòng chọn danh mục cho sản phẩm.";
    public const string ProductImageInvalid = "Đường dẫn ảnh sản phẩm không hợp lệ.";
    public const string PrimaryImageInvalid = "Ảnh đại diện phải nằm trong danh sách ảnh đã chọn.";
    public const string ProductStatusInvalid = "Trạng thái sản phẩm phải là ACTIVE hoặc DISCONTINUED.";
    public static string ProductImagesInvalid =>
        $"Mỗi sản phẩm được có tối đa {ProductLimits.MaximumImages} ảnh.";

    public static string ProductPriceInvalid =>
        $"Giá sản phẩm phải lớn hơn hoặc bằng {ProductLimits.MinimumPrice} đồng.";
}
