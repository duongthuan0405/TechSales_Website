# Tổ chức logic nghiệp vụ tập trung

## 1. Tập trung theo trách nhiệm và nguồn quyết định

Mỗi quy tắc nghiệp vụ có một nơi định nghĩa chính thức. Các lớp khác gọi lại quy tắc đó thay vì viết lại biểu thức. Tập trung không có nghĩa gom tất cả vào một lớp BusinessService: tổ chức theo khái niệm nghiệp vụ và phạm vi áp dụng.

| Loại logic | Nơi sở hữu |
|---|---|
| Invariant của entity/value object | Domain entity/value object |
| Điều kiện thuần cần dùng ở nhiều nơi | Domain/Rules, nếu là quy tắc miền |
| Quy tắc phối hợp nhiều đối tượng miền | Domain policy/service khi cần |
| Điều phối, đọc dữ liệu, quyền tài nguyên, transaction | XXX_UCHandler |
| Điều kiện input chỉ thuộc một UC | XXX_UCInput_Validator tại Application |
| JSON, binding, định dạng transport | Presentation |
| I/O, constraint lưu trữ, concurrency implementation | Infrastructure |

Một handler chịu trách nhiệm một UC nhưng không sở hữu mọi thuật toán của UC. Handler lấy dữ liệu qua port, gọi hành vi Domain, ghi kết quả và điều phối side effect. Không đưa truy vấn database hay phụ thuộc framework vào Domain để làm nó tự điều phối toàn bộ luồng.

## 2. Quy tắc dùng chung giữa Domain và validator

Khi cùng một invariant cần được kiểm tra ở Domain và UCInput_Validator, tách predicate thành rule thuần dùng chung. Code/message có thể hardcode hoặc đặt cùng rule khi hữu ích; không bắt buộc catalog hay resolver. Không tạo interface hoặc hệ thống rule tổng quát khi một static class đã đủ.

```text
Domain/
  Rules/PositiveValueRule.cs
  Entities/...
Application/
  UseCases/<Feature>/<XXX>/
    XXX_UCInput.cs
    XXX_UCOutput.cs
    XXX_UCHandler.cs
    XXX_UCInput_Validator.cs
```

Ví dụ chỉ dùng khi yêu cầu thực tế quy định giá trị phải dương, không mặc định mọi giá trị số đều phải dương:

```csharp
// Domain/Rules/PositiveValueRule.cs
public static class PositiveValueRule
{
    public const string ErrorCode = "VALUE_MUST_BE_POSITIVE";
    public const string ErrorMessage = "Giá trị phải lớn hơn 0.";

    public static bool IsSatisfied(int value) => value > 0;
}
```

Entity/value object gọi rule ở mọi điểm tạo hoặc thay đổi liên quan, và chặn đường ghi trực tiếp có thể bỏ qua invariant:

```csharp
// Trong entity/value object; DomainException thuộc Domain.
public void ChangeValue(int value)
{
    if (!PositiveValueRule.IsSatisfied(value))
    {
        throw new DomainException(PositiveValueRule.ErrorMessage);
    }

    Value = value;
}
```

Validator dùng cùng rule để trả lỗi theo field trước khi handler chạy:

```csharp
public sealed class XXX_UCInput_Validator
    : AbstractValidator<XXX_UCInput>
{
    public XXX_UCInput_Validator()
    {
        RuleFor(x => x.Value)
            .Must(PositiveValueRule.IsSatisfied)
            .WithErrorCode(PositiveValueRule.ErrorCode)
            .WithMessage(PositiveValueRule.ErrorMessage);
    }
}
```

Các đoạn trên minh họa cấu trúc, giả định input/entity có Value phù hợp và DomainException nhận message theo thiết kế tối giản ở mục 4. ErrorCode trên rule là tùy chọn cho validator/contract hiện có, không bắt buộc DomainException có Code. Validator phải chạy qua MediatR validation behavior.

Rule thuần không phụ thuộc FluentValidation, MediatR, HTTP, database hoặc SDK; không I/O hay side effect. Chỉ Application validator biết FluentValidation. Domain vẫn bảo vệ invariant khi được gọi từ job hoặc luồng khác; handler không cần kiểm tra lại lần thứ ba cùng điều kiện thuần.

## 3. Quy tắc phức tạp và phụ thuộc trạng thái

Nếu quy tắc cần nhiều dữ liệu, handler tải dữ liệu cần thiết qua repository port rồi truyền domain object/value vào domain behavior hoặc policy. Không ép mọi quy tắc thành predicate một tham số, và không tạo rule riêng cho mọi dòng if.

Validator không thay thế kiểm tra trạng thái tại thời điểm thực thi hoặc bảo vệ cạnh tranh ghi. Unique constraint, cập nhật có điều kiện, concurrency token hoặc transaction vẫn cần thiết tùy use case. Kiểm tra sớm thành công không chứng minh thao tác ghi sau đó vẫn hợp lệ.

Quyền truy cập endpoint thuộc Presentation; quyền trên tài nguyên và danh tính actor thuộc quá trình điều phối Application. Quy tắc thuần về khả năng thực hiện hành vi có thể thuộc Domain khi phản ánh nghiệp vụ thực tế. Không đưa HttpContext/claims vào entity để kiểm tra quyền.

## 4. Mã lỗi và ánh xạ HTTP


Domain ném DomainException khi vi phạm invariant/quy tắc miền, không quyết định HTTP status. Cho phép message hardcode; không bắt buộc catalog, resolver, code/parameters hoặc exception con.

### Quy ước DomainException

Khi thao tác Domain vi phạm invariant hoặc quy tắc miền, phải ném DomainException. Không trả false/null hoặc Result thất bại thay cho báo lỗi ở các hành vi cần bảo vệ invariant. Predicate như IsSatisfied vẫn trả bool; entity/domain behavior gọi predicate và ném lỗi khi thực hiện thao tác không hợp lệ. Lỗi lập trình/hạ tầng không tự động là DomainException.

Đặt tại Domain/Exceptions/DomainException.cs:

```csharp
namespace YourRootNamespace.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

Thay namespace thực tế, thêm using System nếu cần. Dùng lại class hiện có nếu tương đương, không tạo trùng. Không thêm HTTP status hoặc framework dependency vào class này.

Ví dụ entity:

```csharp
public void ChangeValue(int value)
{
    if (!PositiveValueRule.IsSatisfied(value))
        throw new DomainException("Giá trị phải lớn hơn 0.");

    Value = value;
}
```

Handler gọi Domain rồi để lỗi truyền lên. Không catch chỉ để throw lại, đổi loại hoặc trả thành công. Chỉ catch khi cần cleanup/rollback thuộc trách nhiệm handler; giữ lỗi gốc và ném lại sau cleanup, không để rollback che mất lỗi chính. Nếu transaction behavior/scope đã xử lý, không thêm catch thừa.

Middleware Presentation bắt DomainException trước nhánh lỗi bất ngờ và mặc định trả 422. Chỉ map 409 khi có phân loại xung đột rõ ràng theo contract; không so khớp câu chữ Message để chọn status. Nếu chưa có phân loại, giữ mặc định 422 thay vì đoán nghĩa.

DomainException.Message là thông báo chủ đích an toàn cho client và có thể được trả theo response contract hiện hành. Không đưa dữ liệu nhạy cảm, nội dung exception kỹ thuật hoặc stack trace vào message. Lỗi ngoài dự kiến trả 500 với thông báo chung, log nội bộ. Validator vẫn trả 400 qua pipeline; Domain vẫn bảo vệ invariant ở các đường gọi khác.

Kiểm tra: thao tác hợp lệ cập nhật đúng; invariant sai ném DomainException và không làm đối tượng rơi vào trạng thái sai; handler không nuốt lỗi; middleware trả status/message an toàn. Test gọi handler trực tiếp không chứng minh validation pipeline đã chạy.

Presentation ánh xạ lỗi đã biết theo ý nghĩa. Quy ước mặc định nếu ứng dụng chưa có contract khác:

| Ngữ cảnh lỗi | HTTP status |
|---|---|
| Validator chặn input UC | 400 |
| Không đủ quyền trên tài nguyên | 403 |
| Không tìm thấy tài nguyên | 404 |
| Xung đột trạng thái hoặc dữ liệu | 409 |
| Vi phạm nghiệp vụ khác được nhận diện | 422 |
| Lỗi không nhận diện/ngoài dự kiến | 500 |

Giữ ErrorCode trong field errors khi chuyển lỗi FluentValidation. Cùng rule có thể bị phát hiện ở validator hoặc Domain; status có thể khác theo ngữ cảnh, code vẫn ổn định. Nếu API yêu cầu cùng status cho cùng code ở mọi luồng, cấu hình mapping nhất quán tại Presentation. Mặc định DomainException trả 422; exception kỹ thuật ngoài dự kiến vẫn là 500. Không map mọi ArgumentException thành lỗi client.

## 5. Khi thay đổi nghiệp vụ

1. Xác định thay đổi là invariant miền, chính sách riêng của UC hay hợp đồng transport.
2. Sửa tại nơi sở hữu; nếu là rule thuần dùng chung, sửa predicate tại một nơi.
3. Rà soát consumer để bảo đảm chúng thực sự cùng ý nghĩa. Nếu ngữ cảnh có chính sách khác, tách rule phù hợp thay vì sửa rule chung làm thay đổi mọi UC ngoài ý muốn.
4. Giữ code lỗi ổn định khi ý nghĩa không đổi; thay đổi contract có chủ đích nếu ý nghĩa lỗi đổi. Cập nhật message và HTTP mapping tại các nơi liên quan; không sao chép lại điều kiện nghiệp vụ.
5. Kiểm tra hành vi và giá trị biên: Domain không cho tạo trạng thái sai, validator dùng đúng rule, pipeline chặn handler khi input sai, HTTP trả đúng code/status.

Không viết test chỉ xác nhận tên rule hoặc tên method được gọi. Kiểm tra kết quả quan sát được và các đường gọi có thể bỏ qua validation. Không bắt buộc tạo rule dùng chung nếu invariant chỉ dùng trong Domain và không cần kiểm tra sớm tại validator.
