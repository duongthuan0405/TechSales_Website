# TechSales modules

Mỗi module là một project .NET 10, áp dụng Clean Architecture bằng các thư mục Domain, Application, Infrastructure và Presentation. Contracts công khai nằm trong chính project module. Không tạo project riêng theo layer.

## Ánh xạ phạm vi nghiệp vụ

| Module theo thiết kế | Project |
|---|---|
| Common | CommonModule |
| IdentifyAccess | AuthModule (giữ project và namespace hiện có) |
| Product | ProductModule |
| INVENTORY__DbConnectionString | InventoryModule |
| CART__DbConnectionString | CartModule |
| ORDERING__DbConnectionString | OrderingModule |
| SHIPPING__DbConnectionString | ShippingModule |
| PROMOTION__DbConnectionString | PromotionModule |
| AFTERSALE__DbConnectionString | AfterSaleModule |
| PAYMENTREFUND__DbConnectionString | PaymentRefundModule |
| REVIEWNOTIFICATION__DbConnectionString | ReviewNotificationModule |
| EmailSending | MailModule (giữ project và namespace hiện có) |

Các module chưa có đầy đủ use case/endpoint nghiệp vụ. Persistence đã có 37 storage model, 37 configuration và 10 migration ban đầu theo mô tả v2; chưa áp dụng migration lên database. Domain entity hiện hữu tách biệt với các storage model.

## Project reference

```text
TechSalesBE.WebAPI -> cả 12 module
11 module nghiệp vụ/tích hợp -> CommonModule
CommonModule -> không module nghiệp vụ nào
```

Ma trận phụ thuộc nghiệp vụ trong tài liệu thiết kế không phải project-reference graph. Ví dụ Ordering cần PaymentRefund và PaymentRefund cần Ordering, nhưng không được tạo reference vòng giữa hai project.

Giao tiếp đồng bộ dùng port của consumer tại Application và adapter tại `TechSalesBE.WebAPI/Integration` gọi contract provider. Host reference cả hai module, consumer không reference provider. Adapter chỉ kết nối/chuyển đổi, không giữ quyết định nghiệp vụ. Integration events chỉ dùng khi phù hợp yêu cầu nhất quán và giao nhận.

Project reference không tự kích hoạt tính năng tại runtime. Đăng ký DI, handler/validator discovery, controller/endpoint và adapter khi có implementation thật. Program gọi RegisterModule của từng module. Auth đăng ký JWT và service/repository hiện có; Mail đăng ký SMTP và kiểm tra options. Chỉ đăng ký không thực hiện gửi email hoặc kết nối database.

ProductModule dùng mock Brand/Category tạm qua port do Product sở hữu; xem [ProductModule/README.md](ProductModule/README.md). Key `PRODUCT__DbConnectionString` mới vẫn hỗ trợ fallback `CATALOG__DbConnectionString` trong giai đoạn đổi tên.

## PostgreSQL persistence

Mỗi module nghiệp vụ/tích hợp có DbContext riêng tại `Infrastructure/Persistence`, dùng Npgsql EF Core 10. CommonModule không sở hữu dữ liệu và không có DbContext.

| Project | Context | Default schema | Environment variable |
|---|---|---|---|
| AuthModule | AuthDbContext | identify_access | AUTH__DbConnectionString |
| ProductModule | ProductDbContext | catalog (giữ schema cũ) | PRODUCT__DbConnectionString |
| InventoryModule | InventoryDbContext | inventory | INVENTORY__DbConnectionString |
| CartModule | CartDbContext | cart | CART__DbConnectionString |
| OrderingModule | OrderingDbContext | ordering | ORDERING__DbConnectionString |
| ShippingModule | ShippingDbContext | shipping | SHIPPING__DbConnectionString |
| PromotionModule | PromotionDbContext | promotion | PROMOTION__DbConnectionString |
| AfterSaleModule | AfterSaleDbContext | after_sale | AFTERSALE__DbConnectionString |
| PaymentRefundModule | PaymentRefundDbContext | payment_refund | PAYMENTREFUND__DbConnectionString |
| ReviewNotificationModule | ReviewNotificationDbContext | review_notification | REVIEWNOTIFICATION__DbConnectionString |
| MailModule | MailDbContext | email_sending | MAIL__DbConnectionString |

Mỗi module có `Presentation/ModuleRegistration/ModuleRegistration.RegisterModule(services, configuration)` để tự đăng ký persistence và các thành phần liên quan. Common đăng ký middleware/response infrastructure; các module có controller tự thêm application part. Program chỉ gọi các entrypoint module (dùng class alias để không trùng tên). Không còn hàm gom persistence ở host. Context chỉ scan configuration trong namespace persistence của module mình.

Mỗi module đọc key riêng dạng `AUTH:DbConnectionString`, tương ứng biến môi trường `AUTH__DbConnectionString`. Không dùng ConnectionStrings hoặc fallback toàn cục. Nếu các module dùng chung một database, gán cùng giá trị cho các biến tương ứng. Cấu hình qua environment/user secrets, không commit credential:

Toàn bộ cấu hình được lưu tập trung tại `TechSalesBE/TechSalesBE.WebAPI/.env`; mẫu chung là `.env.example` cùng thư mục. Các key vẫn chia prefix AUTH__, PRODUCT__, CART__, MAIL__... Không còn file mẫu, file .env hay loader riêng trong từng module.

Program xác định môi trường rồi gọi EnvironmentConfiguration.Load đúng một lần trước CreateBuilder; chỉ Development mới nạp .env. Cả 11 factory dùng lại loader Common qua DesignTimeConfiguration, không khởi tạo WebAPI hoặc JWT/SMTP. Loader định vị file chung từ project WebAPI hoặc thư mục chứa TechSalesBE.slnx; bản publish dùng `.env` cạnh assembly. File không tự copy vào publish. Production/Staging bỏ qua file hoàn toàn (kể cả --env-file), chỉ dùng cấu hình environment/CLI có sẵn.

DotNetEnv.NoClobber ưu tiên biến môi trường process có sẵn. Trong Development, thiếu file mặc định thì tiếp tục dùng environment; explicit --env-file bị thiếu thì báo lỗi. Ngoài Development không truy cập file. Không log credential và không commit `.env` thật. Cấu hình environment trực tiếp vẫn được hỗ trợ:
```powershell
$env:AUTH__DbConnectionString = 'Host=localhost;Port=5432;Database=techsales;Username=YOUR_USER;Password=YOUR_PASSWORD'
$env:CART__DbConnectionString = 'Host=localhost;Port=5432;Database=techsales;Username=YOUR_USER;Password=YOUR_PASSWORD'
```

DbConnectionString được kiểm tra khi resolve context; thiếu key module sẽ báo rõ cấu hình cần bổ sung. Host không tự kết nối DB hoặc tạo schema/bảng. MailDbContext là nền tảng dữ liệu tương lai, không tự lưu email/SMTP.

Auth RegisterModule hiện đăng ký JWT nên cần AUTH__JWT__SecretKey (base64), AUTH__JWT__Issuer và AUTH__JWT__Audience. Program bật UseAuthentication trước UseAuthorization. Chính sách permission động chưa bật vì permission repository hiện chưa kiểm tra thực tế; không đổi giới hạn này khi chỉ chỉnh registration.

Mail RegisterModule gọi đăng ký SMTP hiện có, nên cần MAIL__Host, MAIL__FromEmail và các tùy chọn SMTP hợp lệ; giữ kiểm tra options khi startup. Xem MailModule/README.md. Không có SMTP connection hoặc email gửi ra chỉ vì đăng ký service.

Mỗi context dùng `__EFMigrationsHistory` trong schema riêng và migration assembly của module. Khi đã có entity/storage model và Fluent API configuration, tạo migration riêng theo context; mỗi context có IDesignTimeDbContextFactory riêng, không khởi tạo WebAPI/JWT/SMTP. Ví dụ chạy từ root repository:

```powershell
dotnet ef migrations add DescribeNextProductChange --project TechSalesBE/Modules/ProductModule --context ProductDbContext --output-dir Infrastructure/Persistence/Migrations
```

Microsoft.EntityFrameworkCore.Design 10.0.14 được khai báo tại từng module và host, với PrivateAssets=all. EF CLI là local tool 10.0.14, khai báo trong `TechSalesBE/.config/dotnet-tools.json`; commit manifest này để cả team dùng cùng phiên bản, không cần cài global. Package Design không tự cung cấp CLI. Common cũng có package theo cấu hình thống nhất nhưng không có DbContext/migration. Không chạy `EnsureCreated` hoặc tự `Migrate` khi startup; áp dụng migration là thao tác riêng khi có yêu cầu. Transaction của các context độc lập dù dùng cùng database; không mặc định atomic giữa module.

Sau khi clone hoặc khi manifest đổi, đứng tại `TechSalesBE` và chạy:

```powershell
dotnet tool restore
dotnet restore
```

Sau đó đứng tại thư mục chứa csproj của module để chạy CLI local:

```powershell
cd Modules/CartModule
dotnet tool run dotnet-ef -- --version
dotnet ef migrations add DescribeNextCartChange --context CartDbContext --output-dir Infrastructure/Persistence/Migrations
```

`dotnet tool run dotnet-ef` gọi rõ local tool để kiểm tra không dựa vào bản global; `dotnet ef` dùng manifest của thư mục cha. Factory chỉ cần DbConnectionString của module đang thao tác, không cần JWT/SMTP. Migration mới phải được commit cùng Designer và ModelSnapshot; không tự áp dụng vào database của người khác.

## Design-time factories

Cả 11 DbContext có factory public tại `Infrastructure/Persistence/<Module>DbContextFactory.cs`. EF CLI tìm factory và tạo context trực tiếp; RegisterModule/Program không tham gia tạo context design-time. Runtime vẫn dùng DI. Common không có DbContext nên không có factory.

Factory và runtime gọi chung `PersistenceRegistration.ConfigureOptions`, giữ cùng provider, connection key, migration assembly và history schema. Không mở kết nối DB chỉ để tạo context.

Factory dùng `CommonModule.Infrastructure.Configuration.DesignTimeConfiguration`:

1. Xác định environment: tham số --environment, rồi DOTNET_ENVIRONMENT, rồi ASPNETCORE_ENVIRONMENT; nếu không có thì Production. Chỉ Development mới đọc file, Production/Staging bỏ qua --env-file.
2. Trong Development: --env-file chọn file cụ thể (thiếu thì lỗi); nếu không chỉ định, tìm .env chung của WebAPI. Thiếu file mặc định thì dùng environment.
3. Không tìm thấy file mặc định thì dùng environment có sẵn, phù hợp CI/CD.
4. DotNetEnv.NoClobber giữ biến process hiện có; IConfiguration đọc environment và các tham số được chuyển tiếp từ CLI. Tham số CLI được ưu tiên nếu có key trùng, nhưng không đưa credential vào command line/log CI; dùng secrets environment.

Đứng tại thư mục có csproj của module:

```powershell
cd TechSalesBE/Modules/CartModule
# Kiểm tra factory/provider, không kết nối DB:
dotnet ef dbcontext info --context CartDbContext
# Tạo migration từ model (chưa thay đổi DB):
dotnet ef migrations add DescribeNextCartChange --context CartDbContext
# Áp dụng migration vào DB đã cấu hình:
dotnet ef database update --context CartDbContext
# Chọn file .env riêng cho dev nếu cần:
dotnet ef dbcontext info --context CartDbContext -- --env-file ../../TechSalesBE.WebAPI/.env
```

`--output-dir` có thể bỏ khi đã có migration; EF tiếp tục dùng vị trí migration hiện có. Muốn chỉ định nơi lưu migration mới, đường dẫn tính từ project module, ví dụ `--output-dir Infrastructure/Persistence/Migrations`, không thêm tên module lần nữa. Không cần `--startup-project`; giữ package Design tại module và EF CLI local đã restore.

Khi chạy EF CLI dev, đặt DOTNET_ENVIRONMENT=Development trong terminal hoặc chuyển tiếp -- --environment Development. launchSettings.json của WebAPI không tự được dùng khi chạy EF từ project module. Không đặt environment selector trong .env để yêu cầu nạp chính file đó, vì environment được quyết định trước lúc đọc file.

CI/CD đặt DOTNET_ENVIRONMENT=Production và cung cấp `<MODULE>__DbConnectionString` qua secret rồi gọi `dotnet ef database update --project Modules/CartModule --context CartDbContext` từ TechSalesBE; không cần `.env`, JWT hoặc SMTP. Lệnh update thay đổi database thật; chỉ chạy trong job được giao đúng môi trường. Không gọi factory từ API để thay registration DI. `.Database.Migrate()` dùng context mà caller cung cấp, không tự tìm factory.

## Schema v2: 37 bảng

| Module | Số bảng | Migration ban đầu |
|---|---:|---|
| Auth | 7 | InitialAuthSchema |
| Product | 4 | InitialCatalogSchema (tên lịch sử) |
| Inventory | 2 | InitialInventorySchema |
| Cart | 2 | InitialCartSchema |
| Ordering | 3 | InitialOrderingSchema |
| Shipping | 4 | InitialShippingSchema |
| Promotion | 2 | InitialPromotionSchema |
| AfterSale | 7 | InitialAfterSaleSchema |
| PaymentRefund | 3 | InitialPaymentRefundSchema |
| ReviewNotification | 3 | InitialReviewNotificationSchema |

Mỗi bảng có `Infrastructure/Persistence/Models/<Table>DbModel.cs` chỉ chứa dữ liệu và một `Infrastructure/Persistence/Configurations/<Table>Configuration.cs : IEntityTypeConfiguration<TDbModel>`. Tất cả mapping, default và constraint nằm trong configuration, không có data annotation trên model/entity. DbSet của storage model là internal để không xuất implementation dữ liệu làm contract liên module.

Enum đặt trong Domain của module và lưu dạng chuỗi varchar(50). BigInt dùng long, TimeStampZ dùng DateTimeOffset, Date dùng DateOnly. Tên gốc `Invetory`, `Provice`, `CreatedAat`, `ProviceCode`, `UpdateAt` được giữ đúng. UserStatus dùng INACTIVE theo v2 thay cho DELETED của khung Auth trước đó.

Nullability dựa trên cột Constraint trong mô tả: PK/NN là bắt buộc; cột chỉ có DEFAULT mà không NN vẫn nullable. Các default now() là default khi insert, không phải trigger tự cập nhật UpdatedAt. Ứng dụng phải gán UpdatedAt cho lần sửa sau nếu nghiệp vụ cần.

Khóa ngoại vật lý chỉ thêm ở các quan hệ được khai báo FK trong cùng module; các mục được mô tả là FK logic (kể cả ShippingFee.ProviceCode, OrderVoucher.VoucherId, ReturnResultProductUnit.ReturnResultId) vẫn là scalar key. OrderItemUnit dùng FK ghép và cascade theo hướng dẫn v2. Các FK khác mặc định Restrict để không tự xóa dữ liệu liên quan khi chưa có chính sách nghiệp vụ cụ thể.

RepairType có default REPAIR và sentinel được cấu hình tại Fluent API để WARRANTY (giá trị enum 0) không bị thay bằng default khi insert. Model không tự định nghĩa default database.

Để xem SQL trước khi triển khai, với EF CLI 10 đã cài:

```powershell
dotnet ef migrations script --idempotent --project TechSalesBE/Modules/ProductModule --context ProductDbContext
```

Tạo migration hoặc script không tạo bảng trong PostgreSQL. Khi thực sự muốn áp dụng, sau khi kiểm tra database và cấu hình `.env` chung của WebAPI, dùng `dotnet ef database update` với project/context tương ứng; chạy riêng cho từng context có migration. Không chạy cho Common/Mail khi chưa có schema nghiệp vụ được định nghĩa.

## Build solution

```powershell
dotnet build TechSalesBE/TechSalesBE.slnx
```

Tham khảo quy tắc kiến trúc trong `.agents/skills/dotnet-modular-monolith/` và schema nghiệp vụ trong `.agents/skills/techsales-modules/techsales-agent-skill-v2.md` tính từ root repository.
