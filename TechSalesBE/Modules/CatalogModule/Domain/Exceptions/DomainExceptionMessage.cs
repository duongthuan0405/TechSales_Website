namespace CatalogModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public const string InvalidBrand = "Tên thương hiệu không được để trống và không được vượt quá 100 ký tự.";
    public const string InvalidCategory = "Tên danh mục không được để trống và không được vượt quá 100 ký tự.";
}
