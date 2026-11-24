namespace CartModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public const string ProductUnavailable = "Sản phẩm đã ngừng kinh doanh, không thể thêm hoặc cập nhật trong giỏ hàng.";
    public const string InvalidId = "{0} không được để trống.";
    public const string InvalidQuantity = "Số lượng sản phẩm trong giỏ hàng phải lớn hơn 0.";
    public const string QuantityOverflow = "Số lượng sản phẩm trong giỏ hàng vượt quá giới hạn cho phép.";
    public const string InvalidTimestamp = "Thời điểm cập nhật sản phẩm trong giỏ hàng không được trước lần cập nhật trước đó.";
    public const string ItemCartMismatch = "Sản phẩm này thuộc một giỏ hàng khác.";
    public const string DuplicateProduct = "Sản phẩm này đã có trong giỏ hàng.";
    public const string ItemNotFound = "Không tìm thấy sản phẩm trong giỏ hàng.";
}
