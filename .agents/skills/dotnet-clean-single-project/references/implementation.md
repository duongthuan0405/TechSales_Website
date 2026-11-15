# Hướng dẫn triển khai chi tiết

## 1. Hợp đồng một use case

Với tiền tố UC là `XXX`, tạo bốn file tại `Application/UseCases/<Feature>/<XXX>/`:

```text
XXX_UCInput           : IRequest<XXX_UCOutput>
XXX_UCOutput          : kiểu kết quả riêng của UC
XXX_UCHandler         : IRequestHandler<XXX_UCInput, XXX_UCOutput>
XXX_UCInput_Validator : AbstractValidator<XXX_UCInput>
```

Đây là sơ đồ hợp đồng, không phải implementation để sao chép nguyên trạng. Handler triển khai `Task<XXX_UCOutput> Handle(XXX_UCInput request, CancellationToken cancellationToken)` và inject các port cần thiết qua constructor. Output có field có nghĩa, tránh tuple lồng nhiều tầng; UC không trả dữ liệu vẫn có output riêng, có thể là record rỗng.

Endpoint inject ISender, map DTO thành input rồi gọi `await sender.Send(input, cancellationToken)`. Chọn rõ field client được phép nhập, không map toàn bộ payload vào entity/thuộc tính đặc quyền. Danh tính đáng tin cậy không lấy từ payload tự khai báo.

Request/response DTO thuộc Presentation; UCInput/UCOutput thuộc Application; entity/value object thuộc Domain; storage model thuộc Infrastructure. Không dùng chung một kiểu xuyên mọi lớp chỉ để bỏ mapping.

## 2. Validation và lỗi

### BusinessException: một class concrete, phân loại bằng Code

Dùng duy nhất BusinessException kế thừa trực tiếp Exception, không abstract và không tạo exception con chỉ để phân loại lỗi. Code là property chỉ đọc, được gán từ constructor:

```csharp
public class BusinessException : Exception
{
    private Dictionary<string, List<string>> _errors = new();
    public string Code { get; }

    public Dictionary<string, List<string>> Errors =>
        _errors.ToDictionary(error => error.Key, error => error.Value.ToList());

    public void AddError(string key, string error)
    {
        bool isExist = _errors.TryGetValue(key, out List<string>? errorLists);
        if (isExist)
            errorLists?.Add(error);
        else
            _errors.Add(key, new List<string> { error });
    }

    public BusinessException(
        string code,
        string message = "",
        Dictionary<string, List<string>>? errors = null) : base(message)
    {
        Code = code;
        if (errors != null)
            _errors = errors;
    }
}
```

Đặt tại Application/Exceptions hoặc vị trí dùng chung tương đương đang có của ứng dụng. Chỉ thay cơ chế phân loại bằng Code: giữ nguyên Message, Errors getter sao chép dictionary/từng list, AddError và cách nhận dictionary ở constructor. Không thêm parameters, catalog, resolver hoặc thay đổi response contract.

```csharp
throw new BusinessException("NOT_FOUND", "Không tìm thấy tài nguyên.");
```

Middleware đọc Code, không so khớp Message hoặc phân biệt exception con:

| Code | HTTP status tại Presentation |
|---|---|
| BAD_REQUEST | 400 |
| UNAUTHORIZED | 401 |
| FORBIDDEN | 403 |
| NOT_FOUND | 404 |
| CONFLICT | 409 |

Giữ code công khai ổn định. Với code chưa có mapping, dùng chính sách fallback hiện hành. DomainException độc lập, không kế thừa BusinessException ở Application; không đưa HTTP status vào hai exception.

Domain ném DomainException khi invariant/quy tắc miền bị vi phạm, theo [business-logic.md](business-logic.md). Handler không nuốt lỗi hoặc catch chỉ để rethrow/bọc lại. Middleware nhận diện loại này, mặc định trả 422; chỉ map 409 khi có phân loại xung đột rõ ràng. Exception ngoài dự kiến trả 500 với message chung an toàn. Không đặt HTTP status trong Domain hoặc đổi mọi exception .NET thành DomainException.


Tạm thời cho phép hardcode code/message theo hợp đồng phản hồi hiện hành; không bắt buộc catalog, resolver hay thiết kế exception riêng. Không trả trực tiếp lỗi kỹ thuật ngoài dự kiến cho client.

| Kiểm tra | Vị trí |
|---|---|
| Binding, JSON, transport format | Presentation |
| Input UC độc lập HTTP | XXX_UCInput_Validator |
| Tài nguyên tồn tại, quyền sở hữu, điều phối trạng thái | XXX_UCHandler |
| Invariant và chuyển trạng thái nghiệp vụ | Domain |
| Ràng buộc lưu trữ và cạnh tranh ghi | Infrastructure/database |

Validator bắt buộc tồn tại, chỉ thêm rule có căn cứ nghiệp vụ. Presentation validation không thay UC validation; validator không thay invariant hoặc constraint database.

Validation behavior `IPipelineBehavior<TRequest,TResponse>` lấy các `IValidator<TRequest>`, gọi ValidateAsync với cancellation token và gom lỗi theo property. Input sai phải tạo lỗi ứng dụng được middleware map thành HTTP 400 cùng field errors, không chạy handler. Không lặp validation thủ công ở mỗi endpoint/handler.

Không chạy validator song song nếu chúng dùng chung dependency không hỗ trợ concurrent access, chẳng hạn scoped DbContext. Truy vấn trong validator không bảo đảm trạng thái còn đúng lúc ghi; handler/database vẫn phải xử lý race condition.

Mapping lỗi HTTP thông thường: input sai 400, chưa xác thực 401, thiếu quyền 403, không tìm thấy 404, xung đột 409, lỗi bất ngờ 500. Giữ chính sách che giấu tài nguyên và contract hiện hành nếu ứng dụng có quy định khác. Không map mọi ArgumentException/InvalidOperationException thành lỗi client.

Log lỗi nội bộ với trace ID; không lộ stack trace, credential hoặc dữ liệu nhạy cảm ra response/log. Cấu hình bí mật dùng cơ chế configuration của ứng dụng, không hardcode.

## 3. Persistence và mapping

Repository port ở Application phải phục vụ nhu cầu UC; không tạo generic repository nếu chưa có lợi ích cụ thể. Signature dùng domain type hoặc result/projection Application, không trả IQueryable/provider type.

Infrastructure map entity và storage model. Nếu dùng EF Core, cấu hình bảng/cột, precision, index, quan hệ và constraint bằng Fluent API tại Infrastructure. Với detached entity, đọc/cập nhật tracked storage model hoặc dùng cách ghi rõ ràng; đừng giả định EF đang theo dõi Domain object.

Read-only query dùng projection/no-tracking khi phù hợp, chỉ tải quan hệ cần thiết. Filter, sort và pagination thực hiện tại nguồn dữ liệu khi có thể; sort phải ổn định, thêm tie-breaker khi giá trị trùng.

Check trước ghi không thay unique constraint, foreign key hoặc concurrency control. Chọn optimistic concurrency, cập nhật có điều kiện hoặc locking theo yêu cầu, kiểm tra kết quả ghi và xử lý xung đột. Lock trong bộ nhớ của một instance không bảo vệ dữ liệu dùng chung giữa nhiều instance.

Map lỗi provider đã biết tại Infrastructure thành lỗi Application phù hợp; không lộ SQL/SDK exception ra HTTP. Chỉ thêm migration khi schema thực sự đổi và không tự chạy lên môi trường ngoài phạm vi.

## 4. Transaction và side effect

Port UnitOfWork/atomic execution nằm ở Application, implementation tại Infrastructure. Với EF Core, các repository trong cùng unit of work dùng chung scoped DbContext. Read-only UC không mặc định cần transaction tường minh.

Handler sở hữu transaction mặc định. Nếu đã dùng transaction behavior, chỉ behavior sở hữu; không mở/commit thêm tại handler. Validation nên chạy trước khi transaction bắt đầu.

Repository không commit từng phần. Phân biệt thao tác chờ SaveChanges với SQL/bulk update ghi ngay; các bước cần atomic phải cùng transaction. Commit, rollback và dispose phải xử lý nhánh lỗi mà không che lỗi gốc.

Rollback database không khôi phục mọi object/tracking state trong bộ nhớ. Kết thúc scope hoặc làm sạch trạng thái trước retry. Retry có giới hạn, dựa trên tính idempotent và loại lỗi; không thử lại mọi exception.

I/O bên ngoài không tự tham gia transaction database. Chọn cơ chế theo yêu cầu nhất quán:

- Side effect sau commit khi có thể xử lý thất bại riêng.
- Outbox khi cần ghi dữ liệu và ý định phát thông điệp cùng transaction.
- Idempotency/deduplication khi request hoặc sự kiện có thể được gửi lại.
- Compensation/đối soát khi thao tác từ xa thành công nhưng bước cục bộ thất bại.

Không mặc định cài tất cả cơ chế trên. Ghi rõ giới hạn và tránh giữ khóa database qua I/O mạng dài khi có thể tổ chức lại UC.

## 5. Logic dùng chung và adapter

Một handler chịu trách nhiệm một UC không có nghĩa handler tự làm mọi việc. Domain behavior giữ quy tắc nghiệp vụ; port/adapter phục vụ I/O; strategy phù hợp khi có nhiều thuật toán thay thế thực sự.

Không gọi handler trực tiếp hoặc Send lồng nhau chỉ để tái sử dụng code vì dễ làm mờ transaction, quyền và side effect. Tách logic dùng chung về abstraction phù hợp. Nếu điều phối nhiều UC là yêu cầu thực tế, thiết kế rõ ranh giới thay vì phát sinh ngẫu nhiên từ refactor.

SDK, HTTP client, cache, file system và transport nằm ở Infrastructure. Adapter không tự quyết định chính sách nghiệp vụ; Application chỉ biết hợp đồng cần thiết. Không mặc định nhà cung cấp hoặc package cụ thể.

## 6. DI và startup

Program.cs/DI extensions là composition root và được phép biết Infrastructure implementation. Đăng ký MediatR scan assembly chứa handler, FluentValidation scan/đăng ký rõ validator, validation behavior đúng một lần, rồi các port/adapter và persistence cần thiết.

Trong một project, handler và validator thường cùng assembly. Kiểm tra visibility/cơ chế scan để tránh bỏ sót validator. API đăng ký theo phiên bản package hiện có; không nâng package chỉ để khớp snippet. Kiểm tra thứ tự validation trước transaction behavior.

DbContext, repository và UnitOfWork dùng scoped lifetime khi cần chia sẻ unit of work. Không inject scoped dependency vào singleton; adapter khác dùng lifetime theo yêu cầu thư viện.

Exception middleware bao ngoài các thành phần cần xử lý lỗi. Khi có routing/CORS, đặt CORS sau routing, trước authentication/authorization; authentication trước authorization rồi map endpoint. Xác minh pipeline thật, không chỉ sự tồn tại của extension method.

## 7. Kiểm chứng

Build đúng project đích, giữ framework/package trừ khi nhiệm vụ yêu cầu đổi. Chọn kiểm tra theo hành vi và rủi ro:

- Domain: invariant và chuyển trạng thái bị cấm.
- Handler: thành công, dữ liệu thiếu, quyền sai, lỗi giữa chừng.
- MediatR pipeline: input sai trả field errors và handler không chạy. Gọi Handle trực tiếp không chứng minh behavior hoạt động.
- Persistence: mapping, constraint, transaction và concurrency; test double/in-memory provider không chứng minh semantics database đích.
- HTTP: route, JSON contract, auth và status.

Dùng fake adapter khi kiểm tra không cần dịch vụ thật; không tự gây side effect ngoài phạm vi. Khi chỉ sửa tài liệu, kiểm tra skill và liên kết thay vì chạy ứng dụng.
