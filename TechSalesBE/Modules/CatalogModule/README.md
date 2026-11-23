# CatalogModule

Module quản lý Product, ảnh sản phẩm, Brand và Category trong một project Clean Architecture.
Tất cả repository dùng chung CatalogDbContext. IProductReferenceData được triển khai bởi
Infrastructure/Services/ProductReferenceData.cs, đọc dữ liệu thật từ repository Brand/Category.
Host chỉ đăng ký CatalogModule, không còn adapter dữ liệu tham chiếu giả.

## Contract với Cart và Inventory

`Contracts/IProductCatalog` cung cấp snapshot sản phẩm theo danh sách ID, bao gồm
trạng thái, giá, Brand/Category và ảnh. Contract đọc trực tiếp dữ liệu Catalog,
không gọi API BrowseProducts hay Inventory nên không tạo vòng gọi giữa hai module.

API GET Product (public/admin, danh sách/chi tiết) trả thêm thuộc tính:

```json
{ "inventory": { "quantity": 7, "reservedQuantity": 3 } }
```

`quantity` là tồn khả dụng, đã tách khỏi số lượng đang giữ chỗ; không trừ
`reservedQuantity` lần nữa. Sản phẩm chưa có bản ghi Inventory trả cả hai số bằng 0.
Dữ liệu tồn kho được lấy theo lô cho các sản phẩm trên trang hiện tại thông qua
`IProductInventory` và contract `InventoryModule.Contracts.IInventoryQuery`.
Frontend ánh xạ `inventory.quantity` sang `Product.stock`.

Cart và Inventory sở hữu port Application riêng; adapter tại WebAPI/Integration
nối các port với contract. Không thêm project reference giữa các module nghiệp vụ.

Giữ nguyên các route API, mã lỗi CATALOG.*, schema catalog, bảng và ID migration hiện có.
Namespace và metadata migration dùng CatalogModule/CatalogDbContext; không thay đổi schema database.
Cấu hình bằng CATALOG__DbConnectionString. Không chạy migration hay seed khi khởi động.

Chạy test: `dotnet test TechSalesBE/Tests/CatalogModule.Tests/CatalogModule.Tests.csproj`.
Test dùng SQLite tạm, không thay đổi database thật.

## Lọc sản phẩm theo thương hiệu

API danh sách `GET /api/products` và `GET /api/admin/products` hỗ trợ query `brandId` (GUID).
Ví dụ: `/api/products?brandId=<brand-guid>&categoryId=<category-guid>&pageNumber=1&pageSize=20`.
Bỏ `brandId` để lấy tất cả thương hiệu. Bộ lọc kết hợp với keyword/category/status và áp dụng trước phân trang;
brand không tồn tại trả danh sách rỗng, GUID sai định dạng trả 400.

## Cập nhật nhiều ảnh cho một sản phẩm

`PUT /api/admin/products/{id}/images` dùng `multipart/form-data`, dành cho Business Admin và Technical Admin.

- `RetainedImageIds`: lặp lại ID từng ảnh cũ cần giữ. Ảnh không có trong danh sách sẽ bị xóa.
- `Images`: nhiều file mới JPG, PNG hoặc WebP, tối đa 5 MB mỗi file.
- `PrimaryImageIndex`: vị trí ảnh đại diện (từ 0) trong danh sách ảnh giữ lại, sau đó đến ảnh mới. Để trống chọn ảnh đầu tiên.

Tổng số ảnh sau cập nhật tối đa 10. Để xóa hết, gửi form chỉ có `PrimaryImageIndex` với giá trị rỗng.
Ảnh giữ lại phải thuộc sản phẩm và không được trùng ID. Không nhận đường dẫn ảnh từ client.
Chỉ xóa file cũ sau khi lưu database thành công; khi thất bại sẽ dọn file mới.
Form sửa sản phẩm dùng API này sau API cập nhật thông tin; hai thao tác lưu độc lập.
