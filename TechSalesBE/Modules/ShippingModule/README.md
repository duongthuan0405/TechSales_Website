# Shipping: địa chỉ giao hàng

Module dùng dữ liệu `Province` và `Ward` đã được import. Không seed/import lại đơn vị hành chính. Giữ nguyên tên lưu trữ hiện tại như `ShippingAddress.UpdateAt`; Domain và API dùng tên `UpdatedAt`.

## API

| Method | Route | Xác thực | Chức năng |
|---|---|---|---|
| GET | `/api/shipping/provinces` | Công khai | Danh sách tỉnh/thành |
| GET | `/api/shipping/provinces/{provinceCode}/wards` | Công khai | Phường/xã thuộc tỉnh/thành |
| GET | `/api/shipping/provinces/{provinceCode}/fee` | Công khai | Phí giao hàng theo tỉnh/thành |
| GET | `/api/shipping/wards/{wardCode}/fee` | Công khai | Phí giao hàng theo tỉnh/thành của phường |
| PUT | `/api/shipping/provinces/{provinceCode}/fee` | Business Admin / Technical Admin | Tạo hoặc cập nhật cấu hình phí |
| GET | `/api/shipping-address` | Bearer JWT | Địa chỉ chưa xóa của người dùng hiện tại |
| POST | `/api/shipping-address` | Bearer JWT | Tạo địa chỉ, trả 201 |
| PUT | `/api/shipping-address/{id}` | Bearer JWT | Sửa phường và địa chỉ chi tiết |
| PUT | `/api/shipping-address/{id}/default` | Bearer JWT | Chọn địa chỉ mặc định |
| DELETE | `/api/shipping-address/{id}` | Bearer JWT | Xóa mềm địa chỉ |

Body tạo/sửa:

```json
{
  "wardCode": "<mã phường có trong database>",
  "detail": "123 đường ABC"
}
```

Client không gửi UserId hoặc ProvinceCode khi ghi địa chỉ. UserId lấy từ claim `sub`, tỉnh/thành được suy ra từ phường. Kết quả được bọc trong `ApiSuccessResponse<T>`; API danh sách có `data.items`. Dữ liệu địa chỉ có mã/tên tỉnh và phường, chi tiết, cờ mặc định và thời gian tạo/cập nhật.

## Quy tắc và luồng ghi

- Địa chỉ đầu tiên tự làm mặc định; các địa chỉ tiếp theo không tự thay mặc định.
- Chỉ chủ sở hữu được sửa, xóa hoặc chọn mặc định. Địa chỉ không tồn tại/đã xóa trả 404; thao tác trên địa chỉ người khác trả 403.
- Chọn mặc định bỏ cờ mặc định của địa chỉ cũ. Chọn lại cùng địa chỉ vẫn thành công.
- Xóa mềm gán DeletedAt và bỏ cờ mặc định. Nếu xóa mặc định, chọn địa chỉ còn lại sớm nhất theo CreatedAt, rồi Id. Nếu không còn địa chỉ, lần tạo tiếp theo sẽ làm mặc định.
- Detail bắt buộc và tối đa ShippingLimits.AddressDetailMaxLength; mã phường phải hợp lệ và có trong database. Validator và Domain dùng chung ShippingRules.

Handler gọi IExecuteAtomically tại Application/Services và truyền callback nghiệp vụ. Implementation ExecuteAtomically tại Infrastructure/Services mở transaction, chạy callback, SaveChanges và commit một lần. Khi thất bại, implementation rollback, xóa tracking và ném lại lỗi gốc; lỗi rollback được log riêng.

Trong callback, handler yêu cầu repository lấy PostgreSQL advisory transaction lock theo người dùng trước khi đọc/ghi địa chỉ, giúp tuần tự hóa cả trường hợp hai request cùng tạo địa chỉ đầu tiên. Đây là khóa đồng bộ thao tác địa chỉ, không khóa tài khoản hoặc toàn bảng. SQLite test dùng cùng implementation ExecuteAtomically, chỉ bỏ qua khóa đặc thù PostgreSQL.

RegisterModule đăng ký repository, MediatR, validator và controller. ValidationBehavior dùng chung tại Common, được TryAddEnumerable để không chạy lặp khi Auth và Shipping cùng đăng ký.

## Kiểm tra

```powershell
dotnet test TechSalesBE/Tests/ShippingModule.Tests/ShippingModule.Tests.csproj
```

Test dùng TestServer và SQLite trong bộ nhớ, dữ liệu tỉnh/phường giả chỉ thuộc test. Các test kiểm tra API, quyền sở hữu, validation, xóa mềm, chuyển mặc định và rollback; không xác minh advisory lock/concurrency trên PostgreSQL thật. Không chạy migration hoặc truy cập dữ liệu đã import khi thực hiện các test này.

## Phí vận chuyển

ShippingFee được seed 10.000 đồng cho các mã 11–44 qua migration SeedShippingFees. API đọc phí hiện tại từ database, không fallback về giá seed. Tỉnh/thành có bản ghi Fee = 0 vẫn hỗ trợ giao miễn phí; không có bản ghi cấu hình trả HTTP 422 với Code `SHIPPING.DELIVERY_NOT_SUPPORTED`. Tỉnh/phường không tồn tại trả 404. Domain không chấp nhận cấu hình phí âm.

Kết quả tra cứu có `data.provinceCode` và `data.fee`. API PUT nhận body `{"fee": 15000}`; fee bắt buộc có trong JSON, không âm, bằng 0 là miễn phí. Province phải tồn tại. Thiếu cấu hình thì tạo mới và hỗ trợ giao hàng trở lại; đã có thì cập nhật. Luồng ghi dùng IExecuteAtomically, đồng bộ việc tạo/cập nhật cùng tỉnh bằng transaction lock tại Infrastructure.

Quyền sửa phí được kiểm tra tại Controller bằng policy `shipping.fees.manage`, chỉ chấp nhận role Business Admin hoặc Technical Admin. Các tên role là claim công khai do cơ chế Auth cung cấp; Shipping không reference AuthModule. Staff/Customer không được sửa; API đọc vẫn công khai. Không thay đổi luồng đặt hàng của Ordering trong phần này.
