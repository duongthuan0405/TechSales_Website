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

Mỗi UC có đủ bốn lớp trong `Application/UseCases/<Feature>/<XXX>/`:

| Lớp | Hợp đồng |
|---|---|
| XXX_UCInput | IRequest<XXX_UCOutput>, độc lập HTTP |
| XXX_UCOutput | Kết quả riêng, kể cả UC không có dữ liệu |
| XXX_UCHandler | IRequestHandler<XXX_UCInput, XXX_UCOutput>, đúng một handler |
| XXX_UCInput_Validator | AbstractValidator<XXX_UCInput>, được pipeline thực thi |

Controller map DTO -> UCInput, gọi ISender.Send rồi map UCOutput -> response. UC là nội bộ module trừ khi được chủ đích thiết kế làm contract; facade module là lựa chọn mặc định cho giao tiếp đồng bộ bên ngoài. Handler điều phối, Domain bảo vệ invariant, Infrastructure thực hiện I/O.

Domain không biết HTTP, ORM, SDK, MediatR hoặc FluentValidation. Khi Domain và validator cần cùng một điều kiện nghiệp vụ, dùng lại rule thuần thuộc Domain thay vì sao chép biểu thức. Không ép mọi điều kiện input vào Domain. Đọc [module-implementation.md](references/module-implementation.md) khi thêm UC, persistence, exception hoặc registration.

## 5. Giao tiếp và dữ liệu

Mỗi module sở hữu phần dữ liệu của mình và độc quyền ghi. Có thể chung database vật lý nhưng tách mapping/migration và boundary truy cập; không sửa trực tiếp bảng của module khác. Giao tiếp đồng bộ qua facade/contract khi cần kết quả ngay; sự kiện khi muốn phản ứng sau thay đổi và chấp nhận mô hình nhất quán phù hợp.

MediatR Send/Publish trong cùng tiến trình không tự tạo ranh giới module, transaction hoặc đảm bảo giao nhận. Đọc [module-boundaries.md](references/module-boundaries.md) khi có gọi liên module, workflow nhiều module, sự kiện hoặc vấn đề ownership dữ liệu.

## 6. Quy trình agent thực hiện

1. Đọc instruction và cấu trúc repository, project references, host, registrations, một luồng UC thực tế. Xác định module sở hữu thay đổi.
2. Mô tả mục tiêu UC, invariant, dữ liệu đọc/ghi, actor/quyền, side effect và contract liên module nếu có.
3. Nếu cần module khác, định nghĩa port tại Application consumer và dùng adapter Host/Integration gọi contract provider; không thêm reference module consumer -> provider theo mặc định. Kiểm tra đồ thị project và vòng gọi runtime.
4. Triển khai bộ bốn lớp UC, Domain rule, repository/adapter; không tạo project theo layer.
5. Nối DI/endpoint, handler/validator discovery, middleware và config; kiểm tra đường chạy thực tế.
6. Kiểm tra phần thay đổi và các consumer bị ảnh hưởng; build host và module không nằm trong solution nếu có liên quan.
7. Báo cáo module/contract/data thay đổi, kiểm tra đã chạy và phần chưa xác minh; không tuyên bố hoàn tất với stub.

## 7. Tiêu chí hoàn tất

- Một project cho mỗi module, CA phân lớp nội bộ; không phát sinh project theo layer.
- Một host/process/deployment; không tự chuyển sang microservice.
- Quyền sở hữu dữ liệu và contract rõ, không dependency vòng hoặc truy cập implementation module khác.
- Đủ bốn lớp UC, validation chạy, registration đúng, không nhân đôi pipeline/handler.
- Exception chung không kéo code nghiệp vụ riêng về Common; HTTP mapping nằm ở Presentation/host.
- Transaction, side effect, retry và failure semantics có chủ đích cho phần liên quan.
- Có bằng chứng build/kiểm tra phù hợp; review theo [review-and-evolution.md](references/review-and-evolution.md) khi thay đổi boundary hoặc chuyển đổi code.
