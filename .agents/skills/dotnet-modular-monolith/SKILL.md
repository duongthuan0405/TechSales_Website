---
name: dotnet-modular-monolith
description: Thiết kế, triển khai, mở rộng hoặc review backend ASP.NET Core theo Modular Monolith, mỗi module áp dụng Clean Architecture trong một project riêng và dùng MediatR cho use case. Dùng khi phân chia module, thiết kế contract, ghép host hoặc xử lý dữ liệu/giao tiếp liên module; không áp dụng cho yêu cầu chỉ phân lớp một ứng dụng không có ranh giới module.
---

# Modular Monolith: mỗi module một project Clean Architecture

## 1. Tư tưởng triển khai

Một ứng dụng chạy trong một tiến trình và triển khai như một đơn vị, nhưng được chia thành các module có quyền sở hữu nghiệp vụ, dữ liệu và API rõ ràng. Module là đơn vị năng lực nghiệp vụ, không phải tên một layer kỹ thuật hoặc một bảng database.

Mỗi module có **một `.csproj`**, bên trong chia Domain, Application, Infrastructure và Presentation bằng thư mục/namespace. Không tách bốn layer thành bốn project. Một WebAPI host ghép các module và quản lý pipeline chung. Có thể có project dùng chung tối thiểu và project test khi thực sự cần.

Skill này tự chứa đầy đủ quy tắc; không cần tải hoặc chỉnh skill kiến trúc một project khác. Các ví dụ dùng tên ModuleA/ModuleB để độc lập miền nghiệp vụ. Việc dùng skill không tự cho phép tái cấu trúc toàn bộ ứng dụng hoặc thay đổi hợp đồng API/schema ngoài nhiệm vụ.

Các nguyên tắc dẫn đường:

- **Sở hữu trước, chia thư mục sau**: xác định module nào quyết định quy tắc và ghi dữ liệu trước khi viết code.
- **Ẩn implementation, công khai khả năng**: module khác yêu cầu một khả năng qua contract, không lấy repository/entity để tự thao tác.
- **Một tiến trình không có nghĩa một khối phụ thuộc**: tránh gọi xuyên layer, chia sẻ DbContext hoặc vòng tham chiếu vì cùng chạy trong host.
- **Tính nhất quán phải được thiết kế**: transaction cục bộ là mặc định; luồng liên module cần xác định rõ atomicity, side effect và cách phục hồi.
- **Common nhỏ và ổn định**: chỉ chia sẻ cơ chế/hợp đồng có cùng ý nghĩa, không gom mọi entity, rule và code Domain về Common.
- **Triển khai đơn giản theo nhu cầu**: không mặc định broker, HTTP nội bộ, service discovery, database riêng hoặc microservice.

## 2. Cấu trúc tổng thể

```text
src/
  ApplicationHost/
    ApplicationHost.csproj
    Program.cs
    Integration/                 # adapter nối port consumer với contract provider
  Modules/
    ModuleA/
      ModuleA.csproj
      Contracts/
      Domain/
      Application/
      Infrastructure/
      Presentation/
    ModuleB/
      ModuleB.csproj
      Contracts/
      Domain/
      Application/
      Infrastructure/
      Presentation/
  Common/                       # chỉ khi cần dùng chung
    Common.csproj
tests/                          # tạo theo nhu cầu kiểm tra
```

Contracts nằm trong chính project của module theo mặc định. Không tự sinh project Contracts riêng cho mỗi module. Nếu contract reference gây vòng phụ thuộc hoặc cần phân phối độc lập, đọc [module-boundaries.md](references/module-boundaries.md) để chọn giải pháp trong phạm vi được giao.

## 3. Ranh giới và chiều phụ thuộc

```text
Host -> module registration / endpoint mapping
ModuleA -> Common tối thiểu
ModuleB -> Common tối thiểu
Common -> không module nghiệp vụ nào
Host.Integration -> ModuleA.Application.Ports, ModuleB.Contracts
ModuleA -X-> ModuleB             # không tham chiếu trực tiếp theo mặc định
```

Trong từng module:

```text
Domain         -> .NET chuẩn; hợp đồng Domain chung thuần nếu cần
Application    -> Domain, port của chính module, MediatR, FluentValidation
Infrastructure -> Application, Domain; adapter I/O của chính module
Presentation   -> Application, Contracts
Registration   -> các implementation của chính module để ghép DI
```

Reference một module project khiến mọi public type của module đó nhìn thấy được. Đặt implementation `internal` khi phù hợp; chỉ public contract, controller theo yêu cầu framework và registration cần dùng từ ngoài. `internal` không phân tách các layer trong cùng module; phải review import/signature. Không dùng InternalsVisibleTo cho module nghiệp vụ khác để vượt ranh giới; test có thể dùng khi có căn cứ.

Host không chứa nghiệp vụ hoặc truy cập repository module. Module không tham chiếu Host. Không truy cập Domain/Infrastructure/repository/DbContext của module khác. Không gọi MediatR request nội bộ của module khác chỉ vì sender trong cùng DI container có thể tìm handler.

Giao tiếp đồng bộ mặc định dùng consumer-owned port: A định nghĩa interface và input/result theo nhu cầu A, handler A chỉ biết port đó. Adapter tại Host/Integration triển khai port A, gọi contract công khai B và chuyển đổi dữ liệu/lỗi theo hợp đồng. Host reference cả A và B, A không reference B; module không reference host. Adapter chỉ chuyển đổi/kết nối, không chứa quyết định nghiệp vụ. Khi contract B đổi mà nhu cầu A không đổi, sửa adapter thay vì handler A. Thay đổi ý nghĩa nghiệp vụ vẫn có thể đòi hỏi xem lại UC A.

## 4. Clean Architecture bên trong module

Controller WebAPI của mọi module phải kế thừa trực tiếp `Microsoft.AspNetCore.Mvc.ControllerBase`. Không tạo lớp base tùy biến nằm giữa `XXXController` và `ControllerBase`. Các tiện ích HTTP/claims/response dùng chung được viết thành extension method cho `ControllerBase`, đặt tại `CommonModule/Presentation/Extensions` hoặc namespace tương đương của phần dùng chung. Extension không chứa nghiệp vụ, truy vấn database hoặc điều phối use case.

**Bắt buộc trong mọi module:** mọi entity trong `Domain/Entities`, kể cả thư mục con, phải khai báo bằng `class`. Không dùng `record`, `record class` hoặc `record struct` trong mọi trường hợp, kể cả entity chỉ có dữ liệu hoặc không có hành vi. Khi tạo hoặc sửa entity, kiểm tra kiểu khai báo và chuyển entity dạng record trong phạm vi nhiệm vụ sang class; giữ constructor, property và mapping cần thiết, không mặc định duy trì value equality của record. Quy định này dành cho entity; UCInput, UCOutput và DTO vẫn có thể dùng record.

Mỗi UC có đủ bốn lớp trong `Application/UseCases/<Feature>/<XXX>/`:

| Lớp | Hợp đồng |
|---|---|
| XXX_UCInput | IRequest<XXX_UCOutput>, độc lập HTTP |
| XXX_UCOutput | Kết quả riêng, kể cả UC không có dữ liệu |
| XXX_UCHandler | IRequestHandler<XXX_UCInput, XXX_UCOutput>, đúng một handler |
| XXX_UCInput_Validator | AbstractValidator<XXX_UCInput>, được pipeline thực thi |

Controller map DTO -> UCInput, gọi ISender.Send rồi map UCOutput -> response. UC là nội bộ module trừ khi được chủ đích thiết kế làm contract; facade module là lựa chọn mặc định cho giao tiếp đồng bộ bên ngoài. Handler điều phối, Domain bảo vệ invariant, Infrastructure thực hiện I/O.

Mọi quy tắc nghiệp vụ phải được định nghĩa tại Domain/Rules của module sở hữu, bao gồm invariant, chuyển trạng thái, điều kiện theo actor/vai trò và chính sách riêng của UC. Entity giữ trạng thái/hành vi, gọi rule trước khi thay đổi dữ liệu; handler điều phối I/O và gọi rule/entity, validator dùng lại predicate. Không đặt quy tắc nghiệp vụ trong Application/Rules hoặc tự lặp điều kiện trong entity/handler. Domain không biết HTTP, ORM, SDK, MediatR hoặc FluentValidation; rule nhận dữ liệu đã tải, không tự truy vấn repository. Binding, cấu hình vận hành và quyền gọi API ở lớp tương ứng. Đọc [module-implementation.md](references/module-implementation.md) khi thêm UC, persistence, exception hoặc registration.

## 5. Giao tiếp và dữ liệu

### Phân quyền API theo role

Bắt buộc dùng `[Authorize(Roles = RoleAccess.Administrators)]` hoặc hằng số role tương ứng trực tiếp tại Controller/action. Không dùng Authorize(Policy), policy RequireRole hoặc wrapper thay cho Roles; không đăng ký policy nhóm role trong RegisterModule. AllowAnonymous dành cho API công khai, Authorize không có Roles dành cho chức năng chỉ cần đăng nhập. Tên/nhóm role dùng hằng số tại Presentation/Authorization; nhóm là chuỗi tên role cách nhau bằng dấu phẩy. Module không reference module nghiệp vụ khác chỉ để lấy tên role.

Nhiều role trong cùng Roles là OR; nhiều attribute Authorize được kết hợp thành AND. Không xếp nhiều attribute một-role để diễn đạt OR. Mỗi attribute nằm trong một cặp [...] riêng. Đọc role từ danh tính đã xác thực, cấu hình RoleClaimType khớp JWT; không tin role do client tự gửi. Chưa xác thực trả 401, thiếu role trả 403 theo response tiếng Việt hiện có.

Giữ nguyên permission entity, bảng, seed, repository, constants, handler/policy provider và API quản trị để dùng lại sau, nhưng không gắn HasPermission hoặc policy kiểm tra permission lên endpoint trong giai đoạn này. Chỉnh permission không thay đổi quyền truy cập API. Handler không lặp kiểm tra quyền gọi API; các quy tắc sở hữu, actor/đối tượng và trạng thái vẫn định nghĩa tại Domain/Rules và được UC áp dụng. Kiểm thử role được phép/bị cấm, role hiện tại và việc thay permission không vượt qua giới hạn role.

Code/message được tổ chức theo module: Domain/Exceptions/DomainExceptionCode.cs và DomainExceptionMessage.cs; Application/Exceptions/BusinessExceptionMessage.cs; Application/Messages/ResponseMessage.cs cho thành công. Tất cả thông báo gửi người dùng dùng tiếng Việt, không hardcode tại nơi gọi. Message động dùng hàm/property dựa trên hằng số có tên; Domain không tham chiếu catalog Application. Giữ nguyên code công khai và response contract khi đổi nội dung message.

Mỗi module sở hữu phần dữ liệu của mình và độc quyền ghi. Có thể chung database vật lý nhưng tách mapping/migration và boundary truy cập; không sửa trực tiếp bảng của module khác. Giao tiếp đồng bộ qua facade/contract khi cần kết quả ngay; sự kiện khi muốn phản ứng sau thay đổi và chấp nhận mô hình nhất quán phù hợp.

MediatR Send/Publish trong cùng tiến trình không tự tạo ranh giới module, transaction hoặc đảm bảo giao nhận. Đọc [module-boundaries.md](references/module-boundaries.md) khi có gọi liên module, workflow nhiều module, sự kiện hoặc vấn đề ownership dữ liệu.

## 6. Quy trình agent thực hiện

**Thống nhất danh sách trả về trong mọi module:** dùng `List<T>` cho kết quả danh sách của repository, port/service, handler, contract liên module và các thuộc tính danh sách trong UCOutput/response DTO. Hàm bất đồng bộ trả `Task<List<T>>`; kết quả phân trang có `Items` kiểu `List<T>`. Không dùng `T[]`, `IEnumerable<T>` hoặc `IReadOnlyList<T>` thay cho kiểu kết quả danh sách nghiệp vụ này. Truy vấn EF Core lấy danh sách bằng `ToListAsync(cancellationToken)`, không dùng `ToArrayAsync()`; ánh xạ trong bộ nhớ tạo `List<T>` hoặc dùng `ToList()`. Không có dữ liệu thì trả danh sách rỗng, không trả null. Quy ước này không thay đổi kiểu dữ liệu nhị phân `byte[]`, chữ ký bắt buộc của thư viện/framework hoặc kiểu truy vấn nội bộ chưa materialize. Khi sửa hợp đồng danh sách, cập nhật interface, implementation và consumer trong phạm vi nhiệm vụ.

**Cú pháp tường minh:** từ khi triển khai hoặc chỉnh sửa code trong mỗi module, ưu tiên câu lệnh dễ đọc và tên biến thể hiện ý nghĩa. Luôn dùng khối `{ }` cho `if/else` và vòng lặp, không gộp điều kiện và thao tác lên một dòng. Kiểm tra nullable bằng `HasValue`/`Value` hoặc so sánh `null` tường minh; không dùng pattern rút gọn như `is { } last`. Khai báo kiểu biến và kiểu khởi tạo rõ ràng khi giúp người đọc hiểu code, tách biểu thức dài thành các biến trung gian; tránh ternary lồng nhau và cú pháp nén trong logic nghiệp vụ. Khi sửa một phương thức, áp dụng cách viết này cho phương thức đó; không tự refactor phần khác ngoài nhiệm vụ.

**Constructor tường minh trong mọi module và host:** không dùng primary constructor cho `class` hoặc `struct`. Khai báo constructor truyền thống bên trong thân kiểu; dependency được lưu trong field `private readonly` có tên rõ ràng, gán qua constructor và sử dụng qua field đó. Nếu cần gọi constructor lớp cha, dùng `: base(...)` trên constructor truyền thống. Khi sửa hoặc chuyển code, cập nhật mọi tham chiếu tới tham số primary constructor sang field tương ứng, giữ nguyên DI, lifetime và hành vi. Quy định này không cấm positional record cho UCInput/UCOutput/DTO ở những nơi skill cho phép record.

1. Đọc instruction và cấu trúc repository, project references, host, registrations, một luồng UC thực tế. Xác định module sở hữu thay đổi.
2. Mô tả mục tiêu UC, invariant, dữ liệu đọc/ghi, actor/quyền, side effect và contract liên module nếu có.
3. Nếu cần module khác, định nghĩa port tại Application consumer và dùng adapter Host/Integration gọi contract provider; không thêm reference module consumer -> provider theo mặc định. Kiểm tra đồ thị project và vòng gọi runtime.
4. Triển khai bộ bốn lớp UC, Domain rule, repository/adapter; không tạo project theo layer.
5. Nối DI/endpoint, handler/validator discovery, middleware và config; kiểm tra đường chạy thực tế.
6. Kiểm tra phần thay đổi và các consumer bị ảnh hưởng; build host và module không nằm trong solution nếu có liên quan.
7. Báo cáo module/contract/data thay đổi, kiểm tra đã chạy và phần chưa xác minh; không tuyên bố hoàn tất với stub.

## 7. Tiêu chí hoàn tất

Quyền gọi API được kiểm tra tại endpoint bằng Authorize/policy/permission; quy tắc actor được tạo/quản lý tài nguyên nào phải được định nghĩa tại Domain/Rules và được use case áp dụng. Handler không truy vấn lại cùng API permission. Handler map Entity trực tiếp thành UCOutput, Controller bọc output với kiểu cụ thể; không thêm View/Response DTO lặp lại. Không tự thêm session, blacklist, refresh token, queue hoặc outbox ngoài phạm vi nghiệp vụ đã chốt.

- Một project cho mỗi module, CA phân lớp nội bộ; không phát sinh project theo layer.
- Một host/process/deployment; không tự chuyển sang microservice.
- Quyền sở hữu dữ liệu và contract rõ, không dependency vòng hoặc truy cập implementation module khác.
- Đủ bốn lớp UC, validation chạy, registration đúng, không nhân đôi pipeline/handler.
- Exception chung không kéo code nghiệp vụ riêng về Common; HTTP mapping nằm ở Presentation/host.
- Transaction, side effect, retry và failure semantics có chủ đích cho phần liên quan.
- Có bằng chứng build/kiểm tra phù hợp; review theo [review-and-evolution.md](references/review-and-evolution.md) khi thay đổi boundary hoặc chuyển đổi code.
