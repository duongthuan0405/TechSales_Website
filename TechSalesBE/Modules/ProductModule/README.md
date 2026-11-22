# ProductModule

Module sản phẩm: thêm sản phẩm, upload nhiều ảnh, sửa thông tin/trạng thái và truy vấn sản phẩm.
Mỗi use case có UCInput, UCOutput, UCHandler và UCInput_Validator, theo Clean Architecture.

## Brand và Category tạm thời

Product chỉ tiêu thụ BrandId/CategoryId và dữ liệu tham chiếu qua
`Application/Ports/IProductReferenceData.cs`. Không có BrandRepository, CategoryRepository
hoặc use case quản lý Brand/Category trong feat này.

Host đăng ký `TechSalesBE.WebAPI/Integration/MockProductReferenceData.cs` với danh sách mock cố định.
Khi ghép branch Brand/Category, thay adapter mock bằng adapter gọi contract công khai của module đó;
giữ handler Product và các route API hiện tại.

Mock đang dùng ID có sẵn trong database development để không vi phạm các khóa ngoại cũ.
Adapter mock không truy vấn, thêm hoặc cập nhật bảng Brand/Category. Với database khác,
cần cập nhật ID mock cho khớp dữ liệu tham chiếu của môi trường đó.

## Tương thích dữ liệu hiện có

Project, namespace, DbContext, UnitOfWork và test project đã đổi từ Catalog sang Product.
Schema vật lý vẫn là `catalog`, giữ nguyên bảng, khóa ngoại và lịch sử migration hiện có.
Các mã lỗi công khai có prefix `CATALOG.` cũ cũng được giữ để tương thích client.
Các DbModel/configuration Brand/Category của bản gốc được giữ để bảo toàn mapping và snapshot;
đây không phải phần triển khai quản lý Brand/Category mới. Product runtime chỉ đọc/ghi Product và ProductImage.

Biến cấu hình mới: `PRODUCT__DbConnectionString`.
Trong giai đoạn chuyển đổi, vẫn nhận `CATALOG__DbConnectionString` nếu chưa cấu hình key mới.
Không chạy migration hoặc seed khi khởi động.

## Kiểm tra

Từ thư mục repository:

```powershell
dotnet test TechSalesBE/Tests/ProductModule.Tests/ProductModule.Tests.csproj
```

Test sử dụng SQLite tạm và reference port giả lập riêng, không thay đổi database thật.
