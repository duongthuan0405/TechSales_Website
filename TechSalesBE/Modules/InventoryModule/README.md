# InventoryModule

Nhập kho và import Excel kiểm tra các ProductId qua `IInventoryProducts`, được
host nối tới contract Catalog. Import kiểm tra toàn bộ danh sách theo lô trước
transaction ghi dữ liệu; có sản phẩm không tồn tại trả 404 và danh sách ID lỗi
trong `errors.ProductIds`, không ghi một phần workbook.

Nhập kho chỉ kiểm tra tồn tại, không yêu cầu ACTIVE: hàng của sản phẩm ngừng
kinh doanh vẫn có thể được nhập/quản lý trong kho.

`Contracts/IInventoryQuery.GetStocksAsync` trả tồn kho theo danh sách ProductId:
`quantity` là tồn khả dụng; `reservedQuantity` là số lượng đang giữ chỗ.
Sản phẩm chưa có bản ghi không xuất hiện trong kết quả contract; Catalog hiển thị
tồn bằng 0. Contract chỉ đọc, không mở transaction hoặc gọi ngược Catalog.

Các database/module không dùng chung transaction; việc kiểm tra Catalog là
snapshot tại thời điểm nhập, không tạo khóa chéo các database.
