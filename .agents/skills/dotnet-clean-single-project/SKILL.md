---
name: dotnet-clean-single-project
description: Triển khai, mở rộng hoặc review backend ASP.NET Core theo Clean Architecture trong một project ứng dụng, phân lớp bằng thư mục và namespace. Mỗi use case dùng MediatR với UCInput, UCOutput, UCHandler và UCInput_Validator. Áp dụng độc lập miền nghiệp vụ.
---

# Clean Architecture một project với MediatR

## 1. Mục tiêu và phạm vi

Tổ chức backend ASP.NET Core thành Domain, Application, Infrastructure và Presentation trong **một project ứng dụng `.csproj`**. Không tạo project riêng cho từng lớp hoặc module. Project test độc lập được phép khi cần; không phải lớp kiến trúc của ứng dụng.

Skill áp dụng cho mọi miền nghiệp vụ. Entity, use case, trạng thái và chính sách phải được xác định từ yêu cầu thực tế; không suy diễn nghiệp vụ từ hướng dẫn kiến trúc. Chỉ thay đổi phần được giao, không tự chuyển đổi toàn bộ ứng dụng, đổi API, schema, framework hoặc package.

## 2. Tổng quan

Luồng xử lý:

```text
HTTP request
  -> Controller: binding, danh tính, Request DTO -> UCInput
  -> ISender.Send: MediatR pipeline
  -> Validation behavior: UCInput_Validator
  -> UCHandler: điều phối một use case
  -> Domain behavior và Application ports
  -> Infrastructure adapters nếu cần I/O
  -> UCOutput -> Response DTO -> HTTP response
```

Chiều phụ thuộc mã nguồn:

```text
Domain         -> thư viện chuẩn .NET
Application    -> Domain; MediatR; FluentValidation
Infrastructure -> Application, Domain
Presentation   -> Application
Composition root -> các lớp để ghép DI và cấu hình pipeline
```

Application gọi Infrastructure qua interface do Application sở hữu. Domain không biết database, HTTP, MediatR, FluentValidation hay SDK. Composition root được phép biết implementation để đăng ký DI; controller và handler không được viện ngoại lệ này để gọi concrete adapter.

Một assembly không chặn tham chiếu sai lớp ở compile time. `internal` chỉ giới hạn trong assembly, không phân tách thư mục. Khi review, kiểm tra import, signature và lời gọi thực tế, không chỉ cây thư mục.

## 3. Cấu trúc thư mục

```text
ApplicationHost/
|-- ApplicationHost.csproj
|-- Program.cs
|-- Domain/
|   |-- Entities/
|   |-- ValueObjects/
|   |-- Enums/
|   |-- Exceptions/
|   `-- Specifications/
|-- Application/
|   |-- UseCases/<Feature>/<XXX>/
|   |   |-- XXX_UCInput.cs
|   |   |-- XXX_UCOutput.cs
|   |   |-- XXX_UCHandler.cs
|   |   `-- XXX_UCInput_Validator.cs
|   |-- Behaviors/
|   |-- Repositories/
|   |-- Ports/
|   |-- Exceptions/
|   `-- Common/
|-- Infrastructure/
|   |-- Persistence/
|   |   |-- ApplicationDbContext.cs
|   |   |-- Models/
|   |   |-- Configurations/
|   |   `-- Migrations/
|   |-- Repositories/
|   |-- Adapters/
|   `-- DependencyInjection.cs
`-- Presentation/
    |-- Controllers/
    |-- DTOs/
    |-- Middlewares/
    `-- Extensions/
```

Chỉ tạo thư mục có nhu cầu thực tế. Thay `<Feature>` và `<XXX>` bằng tên phù hợp yêu cầu. Namespace theo `<RootNamespace>.<Layer>.<Feature>`. Có thể giữ tên lớp thư mục tương đương của codebase hiện hữu, như `Presentation_WebAPI`; không rename hàng loạt chỉ vì ví dụ này.

## 4. Trách nhiệm từng lớp

| Lớp | Sở hữu | Không đặt tại đây |
|---|---|---|
| Domain | Entity, value object, invariant, hành vi và chuyển trạng thái nghiệp vụ | HTTP, EF Core, DI, SDK, I/O |
| Application | Bộ bốn lớp UC, behavior, port, lỗi ứng dụng, điều phối transaction | Controller, HttpContext, DbContext, storage model, kiểu SDK |
| Infrastructure | Persistence, migration, repository, transaction implementation, adapter | Quyết định nghiệp vụ, DTO HTTP |
| Presentation | Route, binding, DTO, claims mapping, HTTP status, middleware | Query database, gọi SDK trực tiếp, điều phối nghiệp vụ/transaction |

### Domain

**Bắt buộc:** mọi entity trong `Domain/Entities`, kể cả thư mục con, phải khai báo bằng `class`. Không dùng `record`, `record class` hoặc `record struct` trong mọi trường hợp, kể cả entity chỉ có dữ liệu hoặc không có hành vi. Khi tạo hoặc sửa entity, kiểm tra kiểu khai báo và chuyển entity dạng record trong phạm vi nhiệm vụ sang class; giữ constructor, property và mapping cần thiết, không mặc định duy trì value equality của record. Quy định này dành cho entity; UCInput, UCOutput và DTO vẫn có thể dùng record.

Vi phạm invariant hoặc quy tắc miền phải ném DomainException thuộc Domain/Exceptions. Handler để lỗi truyền lên middleware, không catch chỉ để ném lại/bọc loại khác. Domain không chứa HTTP status; Presentation mặc định ánh xạ DomainException thành 422. Xem business-logic.md để triển khai và xử lý ngoại lệ phù hợp.

Entity/value object bảo vệ quy tắc luôn phải đúng trong miền thực tế. Handler điều phối khi cần nhiều đối tượng hoặc dữ liệu bên ngoài. Domain không truy vấn database hoặc thực hiện I/O. Dùng method mang ý nghĩa nghiệp vụ khi thay đổi trạng thái; không bắt buộc Builder, base entity hay domain service cho mọi đối tượng.

### Application và bộ bốn lớp UC

Lỗi cấp UC dùng duy nhất một BusinessException concrete kế thừa Exception, không abstract và không tạo các exception con chỉ để phân loại. Constructor nhận `(string code, string message = "", Dictionary<string, List<string>>? errors = null)`; Code kiểu string, chỉ đọc, gán từ constructor và không được rỗng. Dùng constant từ static class BusinessExceptionCode bên ngoài exception, không dùng enum hoặc nested Codes. Giữ nguyên Message, Errors getter sao chép dictionary/từng list, AddError và cách nhận errors ở constructor. Middleware ánh xạ HTTP theo Code. DomainException độc lập, không kế thừa BusinessException. Xem mẫu đầy đủ trong [implementation.md](references/implementation.md).

Cả BusinessException và DomainException dùng string Code. Danh mục code là static class bên ngoài exception: BusinessExceptionCode cho phân loại dùng chung; DomainExceptionCode thuộc từng feature/module sở hữu nghiệp vụ. Không gom domain code của mọi module vào phần dùng chung. Message vẫn hardcode, không bắt buộc catalog/resolver. Xem implementation.md.


Về cách tập trung quy tắc nghiệp vụ, đọc [business-logic.md](references/business-logic.md). Domain là nguồn quyết định invariant; handler điều phối UC, validator dùng lại rule thuần khi cần kiểm tra sớm, Presentation ánh xạ lỗi sang HTTP. Không sao chép cùng điều kiện nghiệp vụ ở nhiều lớp hoặc gom mọi quy tắc vào một service toàn cục.

Mỗi use case do **đúng một lớp handler** đảm nhận và bắt buộc có đủ bốn lớp trong cùng thư mục:

| Lớp | Hợp đồng |
|---|---|
| `XXX_UCInput` | `IRequest<XXX_UCOutput>`; input độc lập HTTP |
| `XXX_UCOutput` | Kết quả riêng của UC, kể cả khi không có dữ liệu |
| `XXX_UCHandler` | `IRequestHandler<XXX_UCInput, XXX_UCOutput>`; điều phối toàn bộ UC |
| `XXX_UCInput_Validator` | `AbstractValidator<XXX_UCInput>`; kiểm tra input trước handler |

Giữ chính xác hậu tố và dấu gạch dưới. Input/output có thể dùng record. Không thay UCOutput bằng `Unit`. Validator vẫn phải tồn tại khi chưa có rule cần thiết; không bịa quy tắc để lấp đầy lớp.

Controller gọi `ISender.Send`, không inject handler hoặc application service thay thế. Không gom nhiều UC vào một lớp service. Handler được dùng Domain behavior, repository, helper port và strategy dùng chung; không cần tự thực hiện mọi thuật toán.

Không gọi handler trực tiếp hoặc `Send` lồng nhau chỉ để tái sử dụng code. Tách logic dùng chung vào abstraction phù hợp và giữ rõ transaction ownership. Job hoặc adapter khác cũng gửi UCInput qua pipeline khi cần các behavior của UC.

Port nằm ở Application, implementation ở Infrastructure. Không để IQueryable, DbSet, storage model, HttpContext, IActionResult hoặc kiểu SDK xuất hiện trong hợp đồng Application. Truyền CancellationToken qua I/O.

### Infrastructure

Tách storage model và entity nghiệp vụ khi triển khai persistence theo mô hình này. Repository map qua lại; cấu hình lưu trữ nằm tại Infrastructure. Nếu dùng EF Core, đặt DbContext, Fluent API và migration tại đây; Domain không mang annotation EF. Sửa domain object detached không làm EF tự lưu: phải có cơ chế ghi ngược rõ ràng.

Repository không tự commit từng bước của UC. Không bắt buộc một ORM, database, nhà cung cấp hay cơ chế tích hợp cụ thể; chọn theo ứng dụng đích.

### Presentation

Controller WebAPI phải kế thừa trực tiếp `Microsoft.AspNetCore.Mvc.ControllerBase`. Không tạo lớp base tùy biến nằm giữa `XXXController` và `ControllerBase`. Các tiện ích HTTP/claims/response dùng chung được viết thành extension method cho `ControllerBase`, đặt tại `Common/Presentation/Extensions` hoặc namespace tương đương của phần dùng chung. Extension không chứa nghiệp vụ, truy vấn database hoặc điều phối use case.

Map Request DTO -> UCInput và UCOutput -> Response DTO; không dùng entity/storage model trực tiếp làm hợp đồng HTTP. Lấy danh tính từ cơ chế xác thực đáng tin cậy, không tin actor ID do client khai báo cho thao tác cần danh tính hiện tại.

Policy ở endpoint kiểm tra quyền truy cập; handler vẫn kiểm tra quyền trên tài nguyên và điều kiện nghiệp vụ. Dùng response/error contract thống nhất trong ứng dụng; không áp đặt một envelope cụ thể cho mọi dự án.

## 5. Quy trình triển khai cho agent

**Thống nhất danh sách trả về:** dùng `List<T>` cho kết quả danh sách của repository, port/service, handler và các thuộc tính danh sách trong UCOutput/response DTO. Hàm bất đồng bộ trả `Task<List<T>>`; kết quả phân trang có `Items` kiểu `List<T>`. Không dùng `T[]`, `IEnumerable<T>` hoặc `IReadOnlyList<T>` thay cho kiểu kết quả danh sách nghiệp vụ này. Truy vấn EF Core lấy danh sách bằng `ToListAsync(cancellationToken)`, không dùng `ToArrayAsync()`; ánh xạ trong bộ nhớ tạo `List<T>` hoặc dùng `ToList()`. Không có dữ liệu thì trả danh sách rỗng, không trả null. Quy ước này không thay đổi kiểu dữ liệu nhị phân `byte[]`, chữ ký bắt buộc của thư viện/framework hoặc kiểu truy vấn nội bộ chưa materialize. Khi sửa hợp đồng danh sách, cập nhật interface, implementation và consumer trong phạm vi nhiệm vụ.

**Cú pháp tường minh:** từ khi triển khai hoặc chỉnh sửa code, ưu tiên câu lệnh dễ đọc và tên biến thể hiện ý nghĩa. Luôn dùng khối `{ }` cho `if/else` và vòng lặp, không gộp điều kiện và thao tác lên một dòng. Kiểm tra nullable bằng `HasValue`/`Value` hoặc so sánh `null` tường minh; không dùng pattern rút gọn như `is { } last`. Khai báo kiểu biến và kiểu khởi tạo rõ ràng khi giúp người đọc hiểu code, tách biểu thức dài thành các biến trung gian; tránh ternary lồng nhau và cú pháp nén trong logic nghiệp vụ. Khi sửa một phương thức, áp dụng cách viết này cho phương thức đó; không tự refactor phần khác ngoài nhiệm vụ.

**Constructor tường minh:** không dùng primary constructor cho `class` hoặc `struct`. Khai báo constructor truyền thống bên trong thân kiểu; dependency được lưu trong field `private readonly` có tên rõ ràng, gán qua constructor và sử dụng qua field đó. Nếu cần gọi constructor lớp cha, dùng `: base(...)` trên constructor truyền thống. Khi sửa hoặc chuyển code, cập nhật mọi tham chiếu tới tham số primary constructor sang field tương ứng, giữ nguyên DI, lifetime và hành vi. Quy định này không cấm positional record cho UCInput/UCOutput/DTO ở những nơi skill cho phép record.

1. Đọc instruction repository, project file, startup, DI và một luồng tương tự. Xác định project đích, API contract và các tích hợp liên quan.
2. Xác định UC: actor, input/output, quyền, invariant, dữ liệu đọc/ghi và side effect theo yêu cầu thực tế.
3. Tạo/cập nhật Domain, đủ bộ bốn lớp UC và port cần thiết.
4. Triển khai handler, validator, mapping; xác định transaction, cạnh tranh dữ liệu và nhánh lỗi.
5. Triển khai persistence/adapter; thêm migration chỉ khi schema thay đổi.
6. Nối endpoint, DI và pipeline; xác minh validator chạy trước handler.
7. Build và kiểm tra phù hợp rủi ro: thành công, input sai, thiếu quyền, lỗi I/O, rollback/concurrency khi liên quan.
8. Báo cáo hành vi đạt được, file thay đổi, kiểm tra đã chạy và phần chưa xác minh.

Đọc [implementation.md](references/implementation.md) khi triển khai chi tiết. Đọc [migration-and-review.md](references/migration-and-review.md) khi chuyển đổi code hiện hữu hoặc review kiến trúc.

## 6. Tiêu chí hoàn tất

- Hai exception dùng string Code và kế thừa trực tiếp Exception; không dùng enum code, nested Codes hoặc exception con chỉ để phân loại. Type exception dùng chung, code đặc thù do feature/module sở hữu; Common không chứa danh mục Domain của mọi module.

- Một project ứng dụng; không tạo project riêng cho layer/module.
- Mỗi UC đủ bốn lớp đúng tên, đúng một handler, validation thực thi qua pipeline.
- Đúng chiều phụ thuộc; controller mỏng, handler điều phối, Domain giữ invariant, Infrastructure thực hiện I/O.
- Mapping, DI, middleware và transaction được nối vào đường chạy thật; không để stub trong tính năng tuyên bố hoàn tất.
- API/schema được bảo toàn hoặc thay đổi có chủ đích và xử lý tương thích phù hợp.
- Không ép thêm AutoMapper, generic repository, event bus, microservice hoặc tách database đọc/ghi. MediatR không kéo theo các yêu cầu đó.
