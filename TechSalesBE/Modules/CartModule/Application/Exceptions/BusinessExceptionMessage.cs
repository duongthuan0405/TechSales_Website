namespace CartModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string ProductNotFound = "Không tìm thấy sản phẩm.";
    public const string InvalidUserId = "Mã người dùng không được để trống khi truy cập giỏ hàng.";
    public const string InvalidProductId = "Mã sản phẩm phải là UUID hợp lệ và không được để trống.";
    public const string InvalidQuantityToAdd = "Số lượng thêm vào giỏ hàng phải là số nguyên dương.";
    public const string InvalidUpdatedQuantity = "Số lượng mới phải là số nguyên dương.";
    public const string CartNotFound = "Không tìm thấy giỏ hàng của người dùng hiện tại.";
    public const string ItemNotFound = CartModule.Domain.Exceptions.DomainExceptionMessage.ItemNotFound;
}
