# InventoryModule

Nhập kho thủ công và import Excel chỉ dành cho Business Admin và Technical Admin.
Staff gọi hai API này nhận 403; vẫn được xem danh sách unit và tồn kho.

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

## Danh sách unit cho quản lý kho

GET /api/inventory/units: productId (tùy chọn), keyword (tìm chuỗi serial), status (IN_STOCK/RESERVED/SOLD/DAMAGED), pageNumber (mặc định 1), pageSize (mặc định 20, tối đa 100). Response data gồm items (productId, serialNumber, status), totalCount, pageNumber, pageSize. Staff, Business Admin và Technical Admin được truy cập. Filter và pagination thực hiện tại database; không thay đổi schema.


Khôi phục unit: PATCH /api/inventory/{productId}/units/status với serialNumber và status IN_STOCK. Chỉ unit DAMAGED được khôi phục; cộng 1 tồn khả dụng, giữ nguyên tồn giữ chỗ. Khóa bản ghi kho và cập nhật unit/tồn trong cùng transaction; yêu cầu lặp bị từ chối để tránh cộng trùng. Trong hộp thoại Kho, unit hỏng có nút Khôi phục về kho với xác nhận.
