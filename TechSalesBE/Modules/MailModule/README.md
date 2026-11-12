# MailModule

Class library .NET 9 độc lập, có thể copy nguyên thư mục sang hệ thống khác hoặc đóng gói NuGet.

## Kiến trúc

Quy định entity: dùng `class`, mỗi property có private field tương ứng (`_subject` / `Subject`), không dùng auto-property. Logic nghiệp vụ nằm trong Domain; Application điều phối, Infrastructure xử lý kỹ thuật.

- `Domain`: thông điệp email và file đính kèm kiểm tra từng thuộc tính trong `init`; `EmailMessage.Validate()` kiểm tra quy tắc phải có ít nhất một nội dung text/HTML trước khi gửi. Không phụ thuộc thư viện ngoài.
- `Application`: `IEmailService` điều phối gọi Domain kiểm tra rồi gửi qua cổng `IEmailTransport`; chỉ phụ thuộc Domain.
- `Infrastructure`: SMTP qua MailKit, cấu hình SMTP; triển khai cổng Application.
- `DependencyInjection.cs`: composition root để ứng dụng sử dụng đăng ký các thành phần.

Các tầng nằm trong một classlib giống cách tổ chức dự án gốc; ranh giới phụ thuộc được tổ chức theo namespace, không được compiler cưỡng chế bằng các assembly riêng.

## Import

```powershell
dotnet add YourApp.csproj reference path/to/MailModule/MailModule.csproj
```

Trong ứng dụng có Generic Host / ASP.NET Core:

```csharp
using MailModule;

builder.Services.AddMailModule(builder.Configuration);
```

Cấu hình `appsettings.json`:

```json
{
  "Mail": {
    "Host": "smtp.example.com",
    "Port": 587,
    "FromEmail": "noreply@example.com",
    "DisplayName": "My Application",
    "UserName": "noreply@example.com",
    "Security": "StartTls",
    "TimeoutMilliseconds": 30000
  }
}
```

Đặt mật khẩu qua secret của ứng dụng hoặc biến môi trường `Mail__Password` khi host có nạp environment configuration. Không lưu mật khẩu vào source. SMTP không yêu cầu đăng nhập thì bỏ cả UserName và Password. Port 465 thường dùng `SslOnConnect`; `None` chỉ dùng khi chủ động chọn kết nối không mã hóa.

Cũng có thể đăng ký bằng `services.AddMailModule(options => { ... });` hoặc chỉ định section khác qua tham số thứ hai của overload IConfiguration. Cấu hình được kiểm tra khi host khởi động hoặc khi options được resolve.

## Gửi email

Inject `MailModule.Application.Interfaces.IEmailService` vào service của ứng dụng:

```csharp
using MailModule.Application.Interfaces;
using MailModule.Domain.Entities;

public sealed class NotificationService(IEmailService emailService)
{
    public Task NotifyAsync(string recipient, CancellationToken cancellationToken = default)
        => emailService.SendAsync(new EmailMessage
        {
            To = new[] { recipient },
            Subject = "Xác nhận yêu cầu",
            TextBody = "Yêu cầu của bạn đã được tiếp nhận.",
            HtmlBody = "<p>Yêu cầu của bạn đã được <strong>tiếp nhận</strong>.</p>",
            Attachments = new[]
            {
                new EmailAttachment("details.txt", System.Text.Encoding.UTF8.GetBytes("Details"), "text/plain")
            }
        }, cancellationToken);
}
```

Hỗ trợ nhiều người nhận To/Cc/Bcc và CancellationToken. Người nhận là địa chỉ email thuần. Ứng dụng chịu trách nhiệm tạo template và HTML-encode dữ liệu người dùng trước khi chèn vào HTML.

Thư viện không phụ thuộc backend, database hoặc các nghiệp vụ đăng ký/đơn hàng. Mỗi lần gửi tạo một SMTP client riêng. Lỗi validation, cấu hình, SMTP và cancellation được truyền về caller. Không tự retry để tránh gửi trùng; việc SMTP chấp nhận email không bảo đảm người nhận đã nhận vào inbox. Không có queue hoặc background worker tích hợp.

Để dùng nhà cung cấp khác, triển khai `IEmailTransport` và đăng ký trước `AddMailModule` (hoặc thay đăng ký mặc định).

## Build và đóng gói

```powershell
dotnet build MailModule/MailModule.csproj
dotnet pack MailModule/MailModule.csproj -c Release -o artifacts
```
