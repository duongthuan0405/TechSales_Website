using CatalogModule.Domain.Exceptions;
namespace CatalogModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string DuplicateBrandName = "Tên thương hiệu đã tồn tại.";
    public const string BrandNotFound = "Không tìm thấy thương hiệu.";
    public const string InvalidBrandId = "Id thương hiệu không hợp lệ.";
    public const string CategoryNotFound = "Không tìm thấy danh mục.";
    public const string InvalidCategoryId = "Id danh mục không hợp lệ.";

    public static string ProductNameInvalid => DomainExceptionMessage.ProductNameInvalid;
    public const string ProductDescriptionInvalid = DomainExceptionMessage.ProductDescriptionInvalid;
    public static string ProductPriceInvalid => DomainExceptionMessage.ProductPriceInvalid;
    public const string ProductBrandInvalid = DomainExceptionMessage.ProductBrandInvalid;
    public const string ProductCategoryInvalid = DomainExceptionMessage.ProductCategoryInvalid;
    public const string ProductNotFound = "Sản phẩm không tồn tại.";
    public const string ProductStatusInvalid = DomainExceptionMessage.ProductStatusInvalid;
    public const string PaginationInvalid = "Thông tin phân trang không hợp lệ.";
    public static string ProductImagesInvalid => DomainExceptionMessage.ProductImagesInvalid;
    public const string PrimaryImageInvalid = DomainExceptionMessage.PrimaryImageInvalid;
    public const string ImageTypeUnsupported = "Ảnh sản phẩm phải là file JPG, PNG hoặc WebP hợp lệ.";
    public static string ImageSizeInvalid(long maximumBytes) =>
        $"Mỗi ảnh sản phẩm phải có dữ liệu và không được vượt quá {maximumBytes / (1024 * 1024)} MB.";
    public const string ProductReferencesChanged =
        "Thương hiệu hoặc danh mục đã thay đổi. Vui lòng kiểm tra và thử lại.";
    public const string ProductChanged = "Sản phẩm đã thay đổi hoặc bị xóa. Vui lòng tải lại và thử lại.";
}
