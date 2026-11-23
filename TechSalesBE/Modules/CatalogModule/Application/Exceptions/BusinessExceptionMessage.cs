namespace CatalogModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string DuplicateBrandName = "Tên thương hiệu đã tồn tại.";
    public const string BrandNotFound = "Không tìm thấy thương hiệu.";
    public const string InvalidBrandId = "Id thương hiệu không hợp lệ.";
    public const string CategoryNotFound = "Không tìm thấy danh mục.";
    public const string InvalidCategoryId = "Id danh mục không hợp lệ.";
}
