using InventoryModule.Domain.Constants;

namespace InventoryModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public const string InvalidProductId = "Mã sản phẩm không hợp lệ.";
    public const string InvalidStockQuantity = "Số lượng tồn khả dụng và số lượng giữ chỗ phải lớn hơn hoặc bằng 0.";
    public const string InvalidOperationQuantity = "Số lượng phải lớn hơn 0.";
    public const string InsufficientAvailableStock = "Số lượng tồn khả dụng không đủ.";
    public const string InsufficientReservedStock = "Số lượng hàng giữ chỗ không đủ.";
    public const string StockQuantityLimitExceeded = "Số lượng tồn kho vượt quá giới hạn cho phép.";
    public static readonly string InvalidSerialNumber =
        $"Vui lòng nhập số serial không rỗng, tối đa {ProductUnitConstants.MaxSerialNumberLength} ký tự.";
    public const string InvalidProductUnitStatus = "Trạng thái thiết bị không hợp lệ.";
    public const string InvalidProductUnitTransition =
        "Trạng thái hiện tại của thiết bị không cho phép thực hiện thao tác này.";
}
