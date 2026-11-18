# Promotion — quản trị voucher

Các API dưới đây chỉ cho Business Admin và Technical Admin truy cập, khai báo trực tiếp `[Authorize(Roles = RoleAccess.Administrators)]` tại Controller. Không dùng policy nhóm role. Chưa triển khai API dành cho khách hàng hoặc tích hợp Ordering.

| Method | Route | Chức năng |
|---|---|---|
| GET | `/api/admin/vouchers?pageNumber=1&pageSize=20` | Danh sách phân trang, gồm cả voucher đã ngừng |
| GET | `/api/admin/vouchers/{id}` | Chi tiết voucher |
| POST | `/api/admin/vouchers` | Tạo voucher, trả 201 và Location tới API chi tiết |
| PUT | `/api/admin/vouchers/{id}` | Thay toàn bộ cấu hình voucher |
| DELETE | `/api/admin/vouchers/{id}` | Ngừng sử dụng: IsActive = false, không xóa bản ghi |

Body tạo/sửa:

```json
{
  "code": "SALE10",
  "type": "PERCENT",
  "value": 10,
  "maxUsage": 100,
  "minOrderAmount": 500000,
  "startDate": "2026-12-01T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z"
}
```

Code, Type và Value bắt buộc xuất hiện trong JSON. Code được trim và viết hoa, duy nhất không phân biệt hoa/thường, tối đa PromotionLimits.VoucherCodeMaxLength. Type là FIXED hoặc PERCENT, không phân biệt hoa/thường; Value là số nguyên dương theo schema bigint, phần trăm không vượt PromotionLimits.MaximumPercent.

Các trường nullable vẫn được lưu null nếu bỏ trống, không tự điền dữ liệu. Nếu cấu hình MaxUsage thì phải dương và không thấp hơn UsedCount; MinOrderAmount không âm. Nếu có cả hai ngày thì EndDate phải sau StartDate. Thời điểm được đổi về UTC trước khi ghi PostgreSQL. Cách diễn giải các trường null khi áp dụng vào đơn hàng sẽ được xử lý trong phạm vi nghiệp vụ đặt hàng sau này.

Sửa cấu hình giữ nguyên Id, CreatedAt, UsedCount, IsActive và các liên kết OrderVoucher. Sửa voucher đã ngừng không tự kích hoạt lại. Ngừng voucher có thể gọi lại; không xóa dữ liệu, không giảm lượt đã dùng và không thay đổi đơn hàng.

Rule nhỏ và rule tổ hợp nằm tại Domain/Rules, không chứa message. Entity gọi rule trước khi thay đổi trạng thái; validator tái sử dụng predicate. Code/message lỗi Domain đặt tại Domain/Exceptions, message lỗi ứng dụng tại Application/Exceptions và message thành công tại Application/Messages. Toàn bộ message phản hồi bằng tiếng Việt.

Handler dùng IExecuteAtomically tại Application/Services; implementation Infrastructure/Services sở hữu transaction, SaveChanges, commit/rollback. Repository ánh xạ Entity ↔ DbModel, query thường dùng LINQ. Ghi theo mã dùng PostgreSQL advisory transaction lock; sửa/ngừng khóa dòng voucher để tuần tự hóa cập nhật. Test SQLite bỏ qua khóa PostgreSQL, không chứng minh được concurrency trên database thật.

```powershell
dotnet test TechSalesBE/Tests/PromotionModule.Tests/PromotionModule.Tests.csproj
```

Test dùng database trong bộ nhớ, không kết nối database thật. Không thêm/chạy migration cho phần này. Chưa tính tiền giảm, kiểm tra voucher khả dụng cho khách, tăng UsedCount hoặc ghi OrderVoucher từ luồng đặt hàng.
