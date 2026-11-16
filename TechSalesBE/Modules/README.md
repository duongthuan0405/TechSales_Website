# TechSales modules

Mỗi module là một project .NET 10, áp dụng Clean Architecture bằng các thư mục Domain, Application, Infrastructure và Presentation. Contracts công khai nằm trong chính project module. Không tạo project riêng theo layer.

## Ánh xạ phạm vi nghiệp vụ

| Module theo thiết kế | Project |
|---|---|
| Common | CommonModule |
| IdentifyAccess | AuthModule (giữ project và namespace hiện có) |
| Catalog | CatalogModule |
| Inventory | InventoryModule |
| Cart | CartModule |
| Ordering | OrderingModule |
| Shipping | ShippingModule |
| Promotion | PromotionModule |
| AfterSale | AfterSaleModule |
| PaymentRefund | PaymentRefundModule |
| ReviewNotification | ReviewNotificationModule |
| EmailSending | MailModule (giữ project và namespace hiện có) |

Chín project mới là khung kiến trúc, chưa có use case, entity, database hoặc endpoint triển khai. Folder entries trong csproj giữ cấu trúc hiển thị trong IDE và source control mà không cần class giả. Chỉ thêm dependency, contract, registration và bộ bốn lớp UC khi triển khai tính năng thực tế.

## Project reference

```text
TechSalesBE.WebAPI -> cả 12 module
11 module nghiệp vụ/tích hợp -> CommonModule
CommonModule -> không module nghiệp vụ nào
```

Ma trận phụ thuộc nghiệp vụ trong tài liệu thiết kế không phải project-reference graph. Ví dụ Ordering cần PaymentRefund và PaymentRefund cần Ordering, nhưng không được tạo reference vòng giữa hai project.

Giao tiếp đồng bộ dùng port của consumer tại Application và adapter tại `TechSalesBE.WebAPI/Integration` gọi contract provider. Host reference cả hai module, consumer không reference provider. Adapter chỉ kết nối/chuyển đổi, không giữ quyết định nghiệp vụ. Integration events chỉ dùng khi phù hợp yêu cầu nhất quán và giao nhận.

Project reference không tự kích hoạt tính năng tại runtime. Đăng ký DI, handler/validator discovery, controller/endpoint và adapter khi có implementation thật. Auth/SMTP vẫn dùng cơ chế registration hiện có; không tự bật authentication hoặc gửi email khi chỉ dựng khung project.

## Build

```powershell
dotnet build TechSalesBE/TechSalesBE.slnx
```

Tham khảo quy tắc kiến trúc trong `.agents/skills/dotnet-modular-monolith/` và phạm vi nghiệp vụ trong `.agents/skills/techsales-modules/techsales-agent-skill.md` tính từ root repository.
