# AuthModule

Triển khai lại các chức năng tài khoản từ BE cũ theo Clean Architecture trong một project. Mỗi use case ở `Application/UseCases` có đủ `UCInput`, `UCOutput`, `UCHandler`, `UCInput_Validator`; controller gửi request qua MediatR. Repository map giữa Domain và DbModel, trả Entity về handler. Handler ánh xạ Entity trực tiếp thành UCOutput; các cấu trúc con và item danh sách được định nghĩa trong UCOutput của chính use case. Controller đưa dữ liệu kết quả vào response envelope, không ánh xạ qua View hoặc Response DTO trung gian. Không trả Entity/DbModel hoặc hash mật khẩu ra API.

## Chức năng và route

| API | Chức năng |
|---|---|
| `POST /api/auth/register` | Đăng ký Customer ở trạng thái PENDING; gửi OTP xác minh. Đăng ký lại tài khoản PENDING sẽ thay mật khẩu và OTP. |
| `POST /api/auth/verify-email` | Xác minh email bằng OTP, chuyển PENDING → ACTIVE. |
| `POST /api/auth/login` | Đăng nhập, trả JWT, email, roles và expiresAt. |
| `POST /api/auth/forgot-password` | Gửi OTP đặt lại mật khẩu; trả cùng thông báo khi email không tồn tại. |
| `PUT /api/auth/reset-password` | Đổi mật khẩu qua OTP dùng một lần. |
| `PUT /api/auth/change-password` | Đổi mật khẩu bằng mật khẩu hiện tại của người đang đăng nhập. |
| `POST /api/auth/logout` | Thu hồi phiên hiện tại. |
| `GET /api/user/me` | Đọc tài khoản và hồ sơ của người đang đăng nhập. |
| `PUT /api/user-profile` | Cập nhật hồ sơ, nhận multipart/form-data. |
| `GET /api/admin/users/customers` | Danh sách Customer, hỗ trợ pageNumber/pageSize. |
| `GET /api/admin/users/staff` | Business Admin xem Staff; Technical Admin xem Staff và các admin. |
| `POST /api/admin/users` | Tạo tài khoản đã kích hoạt, có email/password/roles. |
| `PUT /api/admin/users/{id}` | Cập nhật email tài khoản trong phạm vi được quản lý. |
| `POST /api/admin/users/{id}/lock` | Khóa tài khoản; body `{ "until": null }` để khóa không thời hạn. |
| `POST /api/admin/users/{id}/unlock` | Mở khóa tài khoản. |
| `GET /api/admin/rbac/roles` | Danh sách role kèm permission. |
| `GET /api/admin/rbac/permissions` | Danh sách permission. |
| `PUT /api/admin/rbac/roles/{id}/permissions` | Thay toàn bộ permission của role bằng permissionIds. |
| `PUT /api/admin/rbac/users/{id}/roles` | Thay toàn bộ role của tài khoản bằng roleIds. |

Các request dùng camelCase JSON như BE cũ. Response thống nhất `{ success, message, data }`; response lỗi có thêm `code`. Tài khoản trả về status dạng chuỗi. Không trả hash mật khẩu, OTP hoặc storage model.

## Bốn role và quyền mặc định

| Role | Quyền mặc định |
|---|---|
| Customer | Đọc/sửa hồ sơ của mình, đổi mật khẩu, đăng xuất. |
| Staff | Thêm quyền xem, khóa và mở khóa Customer. Không quản lý Staff/admin. |
| Business Admin | Xem Customer/Staff; sửa, khóa/mở khóa Customer/Staff; tạo Staff hoặc Business Admin. Không sửa/khóa tài khoản admin ngang cấp hoặc Technical Admin. |
| Technical Admin | Quản lý mọi tài khoản khác; đọc và thay đổi RBAC; tạo tài khoản với bất kỳ role nào trong bốn role. |

Role dùng ID cố định từ BE cũ. Migration `SeedAuthRolesAndPermissions` thêm 4 role, 5 permission thuộc Auth và 11 liên kết RolePermission, không tạo lại bảng.

Phân quyền được thực hiện tại Controller bằng `[Authorize]`, `[HasPermission]` và `IAuthorizationService` với policy quản lý tài khoản đích tại `Presentation/Authorization`. Controller kiểm tra phạm vi role trước khi gửi UCInput qua MediatR, đồng thời chọn phạm vi dữ liệu danh sách Staff theo danh tính đã xác thực. Handler không kiểm tra lặp role/permission và không dùng AuthAccess; handler xử lý nghiệp vụ, trạng thái dữ liệu, transaction và ánh xạ UCOutput. Các entrypoint gọi UC ngoài HTTP phải thực hiện phân quyền tại entrypoint tương ứng.

Danh mục permission ở `Application/Constants/AuthPermission.cs`. `HasPermission` truy vấn quyền thật từ database. Technical Admin có toàn quyền; không sửa permission của role này. Chỉ Technical Admin được gán role hoặc chỉnh RBAC; không tự thay role/khóa chính mình. Đổi role thu hồi phiên của tài khoản đích; đổi permission có hiệu lực ở request kế tiếp.

## Persistence và bảo mật

Các giới hạn nghiệp vụ tập trung tại `Domain/Constants/AuthLimits.cs`: mật khẩu, OTP, đăng nhập sai/khóa tài khoản, hồ sơ, dung lượng avatar, phân trang, số role/quyền và giới hạn request. Domain, validator, service, thông báo email và mapping các trường liên quan cùng dùng các hằng số này. Thay giới hạn độ dài cột cần tạo migration mới; không sửa migration hoặc snapshot lịch sử. Thời hạn JWT dùng hằng số làm mặc định, cấu hình môi trường có thể ghi đè trong khoảng cho phép.

- Giữ bảy bảng hiện tại, mapping bằng `IEntityTypeConfiguration<T>`, schema `identify_access`.
- Email được trim/lowercase khi ghi; mật khẩu BCrypt cost 12, tối thiểu 8 ký tự, tối đa 72 byte UTF-8, tương thích thuật toán BE cũ.
- OTP ngẫu nhiên sáu chữ số, hạn năm phút, dùng một lần. Bảng `UserOtp` lưu SHA-256 của OTP có gắn user/type, không lưu mã nguyên văn.
- JWT HS256 kiểm tra issuer/audience/lifetime/signature và phiên trong database. Secret cấu hình là Base64 của ít nhất 32 byte ngẫu nhiên.
- `UserSession` lưu SHA-256 của JWT jti; mỗi tài khoản có một phiên hoạt động. Đăng nhập mới thay phiên cũ. OTP được lưu riêng trong `UserOtp`.
- Đăng xuất, đổi/reset mật khẩu, khóa tài khoản, đổi email/role thu hồi phiên. JWT còn hạn nhưng phiên đã bị thu hồi sẽ nhận 401. Authentication đọc role hiện tại từ database.
- Năm lần sai mật khẩu trong cửa sổ hoạt động dẫn đến khóa 30 phút. Counter được commit trước khi trả lỗi. Hạn khóa tự động được xét lại khi đăng nhập; reset mật khẩu không gỡ khóa thủ công.
- `LastLoginFailedAt` ghi riêng thời điểm đăng nhập sai gần nhất, dùng để xét cửa sổ của chuỗi thất bại theo `AuthLimits.FailedLoginWindowMinutes`. Thay đổi hồ sơ/tài khoản không ảnh hưởng mốc này. Đăng nhập thành công, đổi/reset mật khẩu hoặc mở khóa sẽ xóa mốc và reset counter. Sau migration, tài khoản cũ chưa có mốc sẽ bắt đầu chuỗi đếm mới ở lần đăng nhập sai tiếp theo.
- Transaction thuộc từng handler qua `IExecuteAtomically`. Repository không tự commit. PostgreSQL khóa row khi đọc/sửa tài khoản hoặc thay permission để tránh ghi đè từ request đồng thời.
- Endpoint Auth công khai giới hạn tổng 20 request/phút/IP trong mỗi instance ứng dụng. Khi chạy nhiều replica, cần cấu hình thêm giới hạn dùng chung tại gateway nếu cần.
- Hồ sơ nhận fullName, phone (10–11 chữ số), dateOfBirth, avatarUrl hoặc avatarFile. Avatar PNG/JPEG/WebP tối đa 5 MB, kiểm tra chữ ký file, đặt tên ngẫu nhiên và lưu dưới `wwwroot/avatars`. Production cần volume bền vững hoặc thay adapter `IAvatarStorage`.
- Gửi email nằm trong transaction đăng ký/reset challenge: lỗi SMTP làm rollback dữ liệu. SMTP không tham gia transaction DB; nếu gửi thành công nhưng commit thất bại, email có thể chứa mã không dùng được. Chưa triển khai outbox.
- Không sao chép Redis/cache hoặc bảng AuditLog ngoài schema mới vào AuthModule. Các chức năng trên hoạt động mà không cần các thành phần đó.

## Mail và ranh giới module

Auth chỉ tham chiếu Common. `IAuthEmailSender` là port do Auth sở hữu; `TechSalesBE.WebAPI/Integration/AuthEmailAdapter.cs` nối port tới public API hiện có của MailModule. Không thêm project reference Auth → Mail. Cấu hình SMTP qua các biến MAIL trong `.env.example`.

`AUTH__FrontendUrl` tùy chọn để email kèm đường dẫn giao diện verify/reset. Nếu không có, email vẫn chứa OTP và thời hạn sử dụng.

## Migration và tài khoản Technical Admin đầu tiên

Đứng tại `TechSalesBE/Modules/AuthModule`:

```powershell
dotnet ef database update -- --environment Development
```

Đã có migration bảng và seed; không cần chạy `migrations add` lần nữa. CI/CD dùng environment/secret của container và bỏ `--environment Development`. Ứng dụng không tự chạy migration khi khởi động.

Để tạo Technical Admin đầu tiên, sau khi migrate đặt hai biến trong môi trường hoặc `.env` Development của WebAPI:

```dotenv
AUTH__Bootstrap__Email="YOUR_TECHNICAL_ADMIN_EMAIL"
AUTH__Bootstrap__Password="YOUR_STRONG_PASSWORD"
```

Khởi động WebAPI một lần rồi bỏ hai biến này. Không có tài khoản/mật khẩu mặc định. Bootstrap không tự nâng quyền một email đã tồn tại và không thay mật khẩu tài khoản đã tạo; nếu email đã là Technical Admin ACTIVE thì không tạo trùng.

## Kiểm thử

```powershell
dotnet test TechSalesBE/Tests/AuthModule.Tests/AuthModule.Tests.csproj
```

Test HTTP dùng TestServer và SQLite riêng, không truy cập PostgreSQL/SMTP thật. Có kiểm tra model PostgreSQL khớp snapshot và sinh SQL migration offline. Row lock PostgreSQL và việc gửi SMTP thật cần kiểm tra trong môi trường tích hợp.
