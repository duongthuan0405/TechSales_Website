# Triển khai Clean Architecture bên trong một module

## 1. Cấu trúc một project

```text
ModuleA/
  ModuleA.csproj
  Contracts/
  Domain/
    Entities/
    ValueObjects/
    Rules/
    Exceptions/DomainExceptionCode.cs
  Application/
    UseCases/<Feature>/<XXX>/
      XXX_UCInput.cs
      XXX_UCOutput.cs
      XXX_UCHandler.cs
      XXX_UCInput_Validator.cs
    Repositories/
    Ports/
  Infrastructure/
    Persistence/
      ModuleADbContext.cs
      Models/
      Configurations/
      Migrations/
    Repositories/
    Adapters/
  Presentation/
    Controllers/
    DTOs/
    ModuleRegistration/
```

Chỉ tạo thư mục/class cần thiết. Namespace theo `<Module>.<Layer>.<Feature>`. Contracts và registration là public API có chủ đích; giữ implementation internal nếu framework/DI hỗ trợ. Thư mục không cưỡng chế chiều phụ thuộc trong một assembly.

## 2. Một UC, một handler, đủ bốn lớp

| Thành phần | Trách nhiệm |
|---|---|
| XXX_UCInput : IRequest<XXX_UCOutput> | Input nghiệp vụ không phụ thuộc HTTP |
| XXX_UCOutput | Kết quả có tên field rõ; vẫn có class/record riêng nếu không có dữ liệu |
| XXX_UCHandler : IRequestHandler<XXX_UCInput, XXX_UCOutput> | Đúng một handler điều phối UC |
| XXX_UCInput_Validator : AbstractValidator<XXX_UCInput> | Validation input trước handler |

Giữ chính xác hậu tố/dấu gạch dưới; không thay output bằng Unit. Validator vẫn tồn tại khi chưa có rule cần thiết nhưng không bịa rule. Input/output/handler là implementation module, không tự đưa vào Contracts.

Controller map request, lấy actor từ danh tính tin cậy, gọi ISender.Send và map output ra response. Handler kiểm tra quyền trên tài nguyên, lấy dữ liệu qua repository/port, gọi Domain behavior, ghi và điều phối transaction/side effect. Không inject DbContext hoặc provider SDK vào handler.

Validation behavior gọi ValidateAsync và gom lỗi theo field, dừng trước handler khi input sai. Không chạy validator dùng chung DbContext song song. Không lặp validation thủ công ở từng endpoint/handler. Handler nội bộ không gọi Send lồng nhau chỉ để dùng lại code; facade của chính module có thể gửi UC như entrypoint có chủ đích.

## 3. Nghiệp vụ và persistence

Entity/value object giữ invariant và hành vi; domain rule thuần dùng chung với validator khi cùng một điều kiện cần kiểm tra sớm. Domain không I/O và không biết MediatR/FluentValidation/EF/HTTP. Tách chính sách chỉ thuộc UC ở Application thay vì đẩy mọi if vào Domain.

Repository port thuộc Application, implementation Infrastructure. Không trả IQueryable, storage model, HTTP type hoặc SDK type qua port. Khi dùng persistence theo cấu trúc này, tách entity Domain và storage model; repository map hai chiều, cấu hình database tại Infrastructure. Với entity detached, ghi lại qua storage model tracked hoặc cơ chế tường minh, không giả định EF tự lưu Domain object.

Port gọi năng lực module khác cũng thuộc Application consumer nhưng implementation đặt tại Host/Integration, không ở project consumer nếu cần tránh consumer reference provider. Handler chỉ dùng model/port của chính module. Adapter host gọi public contract provider và map kết quả/lỗi, không truy cập repository hoặc quyết định nghiệp vụ. Xem ví dụ tại [module-boundaries.md](module-boundaries.md).

Query filter/sort/page ở data source khi phù hợp, dùng projection/no-tracking cho đọc khi cần. Unique/FK/concurrency constraint không được thay bằng check trước ghi. Giữ stock/state/data race theo quy tắc thực tế, không mặc định một miền nghiệp vụ.

Một UC có một owner transaction: handler hoặc transaction behavior, không cả hai. Repositories không commit từng bước. Các repository cần atomic phải cùng resource/transaction thật. Rollback không tự reset object/tracking state; cleanup không được che lỗi gốc. I/O ngoài DB không tự rollback cùng.

## 4. Common và exception

Common chỉ chứa cơ chế/hợp đồng thực sự dùng chung, không chứa entity/rule/repository hay danh mục Domain code của mọi module. Domain dùng chung chỉ tham chiếu phần thuần, không import HTTP/persistence. Một project Common chứa nhiều namespace không tự cưỡng chế quy tắc này.

Dùng hai exception độc lập kế thừa trực tiếp Exception:

| Loại | Nơi sở hữu type | Hợp đồng |
|---|---|---|
| DomainException | Domain chung thuần | string Code, Message; giữ overload InnerException nếu có |
| BusinessException | Application chung hoặc vị trí Common tương đương | string Code, Message, Errors, AddError |

Domain ném DomainException khi vi phạm invariant; predicate vẫn có thể trả bool để entity quyết định ném lỗi. BusinessException là một class concrete duy nhất, không abstract/exception con chỉ để phân loại. Không kế thừa DomainException từ BusinessException.

Constructor BusinessException là `(string code, string message = "", Dictionary<string, List<string>>? errors = null)`. Giữ Errors getter sao chép dictionary và từng list, AddError thêm theo key, constructor nhận errors theo hành vi hiện có. Không tự đổi cách ownership dictionary hoặc response contract trong một thay đổi chỉ về code exception.

Code không rỗng; static class **nằm ngoài exception**, không enum hoặc nested Codes:

```text
Common/Application/Exceptions/BusinessExceptionCode.cs
ModuleA/Domain/Exceptions/DomainExceptionCode.cs
ModuleB/Domain/Exceptions/DomainExceptionCode.cs
```

Mã Business phổ biến dùng chung như BAD_REQUEST, UNAUTHORIZED, FORBIDDEN, NOT_FOUND, CONFLICT. Mã Domain do module sở hữu, có namespace/prefix ngữ nghĩa khi cần tránh trùng. Module dùng chung không kiểm tra registry toàn cục các mã Domain. Không chuyển code module về Common để dễ catch exception.

```csharp
// Trong Domain của module sở hữu quy tắc:
public static class DomainExceptionCode
{
    public const string INVALID_STATE = "MODULE_A.INVALID_STATE";
}

// Trong hành vi Domain:
throw new DomainException(
    DomainExceptionCode.INVALID_STATE,
    "Không thể thực hiện thao tác ở trạng thái hiện tại.");
```

Tên code/câu chữ chỉ minh họa. Dùng namespace thực tế, không suy ra mọi module cần rule trên. Message có thể hardcode; chưa bắt buộc catalog/resolver. Handler không nuốt/bọc DomainException chỉ để đổi loại; cleanup/rollback khi cần vẫn giữ lỗi gốc.

Middleware chung tại Presentation/host nhận BusinessException, map code 400/401/403/404/409; DomainException mặc định 422. Mapping Domain khác nếu cần ở composition/Presentation biết module, không ở shared exception. Lỗi bất ngờ hoặc Business code không nhận diện dùng 500 an toàn theo contract; không lộ stack trace/exception kỹ thuật. Không chọn status bằng Message. Giữ code string trong response khi API yêu cầu.

Middleware không ghi lại body nếu response đã bắt đầu; cancellation do request hủy không biến thành lỗi 500 thông thường. Authentication challenge/model binding không nhất thiết ném exception; nếu cần envelope đồng nhất cho các đường này, cấu hình handler/filter tương ứng trong phạm vi được giao.

## 5. Registration và host

Mỗi module công khai entrypoint như RegisterModuleA hoặc AddModuleA. Entry point được biết implementation của chính module để bind DI, options, persistence, facade và endpoint/controller. Host gọi các entrypoint, không sao chép đăng ký chi tiết hoặc nghiệp vụ module.

Host có registration Integration riêng để bind port consumer với adapter gọi provider. Việc đăng ký adapter là composition, không phải đưa nghiệp vụ vào host. Không yêu cầu registration module biết adapter host; module không reference host. Dùng scoped adapter khi provider/facade có scoped dependency và bảo đảm mọi port cần thiết đã được bind.

Kiểm tra tương thích target framework trước khi thêm project reference; không nâng framework/package ngoài nhu cầu được giao. Không cố gắn module vào host chỉ vì module đã có project.

Với MediatR dùng chung container:

- Xác định đúng assembly handler/validator của từng module và đảm bảo chúng được discovery.
- Tổng hợp assembly hoặc đăng ký theo cách thư viện hỗ trợ; xác minh phiên bản hiện có thay vì giả định mỗi lần AddMediatR đều an toàn.
- Validation/transaction behavior dùng chung chỉ đăng ký một lần; tránh mỗi module thêm cùng behavior vào pipeline khiến validate/commit lặp.
- Một request có đúng một handler. Type request phải thuộc đúng namespace/module.
- Nếu handler/validator internal, xác minh cơ chế scan/DI thật sự hỗ trợ; đăng ký rõ khi cần thay vì public hóa toàn bộ module.

Controllers ở module assembly cần được MVC discovery, có thể AddApplicationPart qua marker type nếu discovery mặc định không đủ. Nếu dùng minimal endpoints, cung cấp mapping entrypoint rõ ràng. Không bắt buộc đổi kiểu endpoint hiện hành.

Host đăng ký global exception middleware, đặt bao ngoài các bước cần xử lý lỗi; authentication trước authorization; routing/CORS đúng thứ tự khi dùng. DbContext/repository/UnitOfWork dùng scope phù hợp; không inject scoped service vào singleton. Shared facade không được cache request state trong singleton.
