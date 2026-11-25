# Tổ chức logic nghiệp vụ tập trung

## 1. Tập trung theo trách nhiệm và nguồn quyết định

Mỗi quy tắc nghiệp vụ có một nơi định nghĩa chính thức. Các lớp khác gọi lại quy tắc đó thay vì viết lại biểu thức. Tập trung không có nghĩa gom tất cả vào một lớp BusinessService: tổ chức theo khái niệm nghiệp vụ và phạm vi áp dụng.

| Loại logic | Nơi sở hữu |
|---|---|
| Invariant của entity/value object | Domain/Rules; entity/value object gọi để bảo vệ trạng thái |
| Điều kiện thuần cần dùng ở nhiều nơi | Domain/Rules, nếu là quy tắc miền |
| Quy tắc phối hợp nhiều đối tượng miền, chính sách riêng UC | Domain/Rules |
| Điều phối, đọc dữ liệu, transaction | XXX_UCHandler |
| Quy tắc sở hữu/quản lý tài nguyên | Domain/Rules; handler cung cấp actor và dữ liệu |
| Binding/định dạng input, gọi predicate nghiệp vụ | XXX_UCInput_Validator tại Application; predicate nghiệp vụ thuộc Domain/Rules |
| JSON, binding, định dạng transport | Presentation |
| I/O, constraint lưu trữ, concurrency implementation | Infrastructure |

Một handler chịu trách nhiệm một UC nhưng không sở hữu mọi thuật toán của UC. Handler lấy dữ liệu qua port, gọi hành vi Domain, ghi kết quả và điều phối side effect. Không đưa truy vấn database hay phụ thuộc framework vào Domain để làm nó tự điều phối toàn bộ luồng.

## 2. Rule con, rule tổ hợp và nơi sử dụng

Một rule có thể tổ hợp nhiều rule con bằng lời gọi method, toán tử logic hoặc rẽ nhánh. Không bắt buộc một rule tương ứng một class/interface riêng. Rule không sở hữu message và không bắt buộc có ErrorCode, ErrorMessage, IsSatisfied hoặc EnsureSatisfied. Có thể chọn predicate trả bool, kết quả có cấu trúc, đối tượng được chọn hoặc guard ném exception theo nhu cầu hiện có. Không tự áp đặt một hình thức duy nhất hoặc thêm hệ thống Result/rule engine.

Kiểm tra kết quả của rule ở entity/handler/validator không phải lặp nghiệp vụ; viết lại biểu thức điều kiện mà rule đã sở hữu mới là lặp nghiệp vụ. Nếu rule trả kết quả thất bại, nơi gọi phải xử lý trước khi thay đổi trạng thái.

Mọi điều kiện và quyết định nghiệp vụ được định nghĩa tại Domain/Rules, kể cả chính sách chỉ dùng cho một UC. Entity giữ state và gọi rule; handler cung cấp dữ liệu và điều phối, không tự viết lại điều kiện. Rule không gọi repository, framework hoặc I/O. Domain không tham chiếu Application; constant mà rule cần phải thuộc Domain/Constants. Giới hạn kỹ thuật, rate limit, SMTP, JWT, timeout và kích thước request thuộc options/constants Infrastructure/Presentation; không đưa vào Domain chỉ vì có giá trị số.

Các con số giới hạn nghiệp vụ phải được định nghĩa một lần bằng hằng số có tên rõ nghĩa tại `Domain/Constants` của feature/module sở hữu, không hardcode lặp trong entity, rule, validator, handler, service hoặc thông báo. Các nơi kiểm tra cùng quy tắc dùng chung predicate tại Domain/Rules và các hằng số này. Nếu thay đổi giới hạn tác động schema, cập nhật mapping và tạo migration mới; không sửa migration/snapshot lịch sử. Không đưa quy tắc đặc thù của mọi module vào Common.

Khi cùng một invariant cần được kiểm tra ở Domain và UCInput_Validator, dùng lại predicate/rule tổ hợp. Rule không khai báo message, kể cả alias hoặc property chỉ chuyển tiếp message. Nơi báo lỗi tham chiếu DomainExceptionCode và catalog message theo layer sở hữu. Guard hiện có có thể tham chiếu catalog Domain khi ném lỗi, nhưng không biến message/exception thành yêu cầu bắt buộc của mọi rule. Mọi nội dung người dùng đọc bằng tiếng Việt. Không tạo interface/resolver tổng quát khi static class đủ dùng.

```text
Domain/
  Rules/ValueRules.cs
  Entities/...
Application/
  UseCases/<Feature>/<XXX>/
    XXX_UCInput.cs
    XXX_UCOutput.cs
    XXX_UCHandler.cs
    XXX_UCInput_Validator.cs
```

Ví dụ rule tổ hợp kiểm tra khoảng giá trị, chỉ dùng khi nghiệp vụ thực tế có yêu cầu này:

```csharp
// Domain/Rules/ValueRules.cs
public static class ValueRules
{
    public static bool IsAtLeast(int value, int minimum)
    {
        return value >= minimum;
    }

    public static bool IsAtMost(int value, int maximum)
    {
        return value <= maximum;
    }

    public static bool IsWithinRange(int value, int minimum, int maximum)
    {
        bool meetsMinimum = IsAtLeast(value, minimum);
        bool meetsMaximum = IsAtMost(value, maximum);
        return meetsMinimum && meetsMaximum;
    }
}
```

Rule trên không chứa code, message hoặc exception. Các giới hạn được truyền vào; nếu là giới hạn nghiệp vụ cố định thì đặt tại Domain/Constants. Entity/value object gọi rule trước khi thay đổi trạng thái. Ví dụ nơi gọi chọn báo lỗi bằng exception:

```csharp
// Trong entity/value object; DomainException thuộc Domain.
public void ChangeValue(int value)
{
    bool isValid = ValueRules.IsWithinRange(value, ValueLimits.Minimum, ValueLimits.Maximum);
    if (!isValid)
    {
        throw new DomainException(DomainExceptionCode.VALUE_OUT_OF_RANGE, DomainExceptionMessage.ValueOutOfRange);
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
        RuleFor(input => input.Value)
            .Must(value => ValueRules.IsWithinRange(value, ValueLimits.Minimum, ValueLimits.Maximum))
            .WithMessage(BusinessExceptionMessage.ValueOutOfRange);
    }
}
```

Các đoạn trên minh họa cách tái sử dụng một rule tổ hợp, giả định ValueLimits và catalog có các thành viên phù hợp. Cách báo lỗi do ngữ cảnh chọn, không phải cấu trúc bắt buộc của rule. Validator chạy qua MediatR validation behavior. Không đổi các rule hiện có sang guard hoặc thêm message chỉ để giống ví dụ.

Rule thuần không phụ thuộc FluentValidation, MediatR, HTTP, database hoặc SDK; không I/O hay side effect. Chỉ Application validator biết FluentValidation. Domain vẫn bảo vệ invariant khi được gọi từ job hoặc luồng khác; handler không cần kiểm tra lại lần thứ ba cùng điều kiện thuần.

Nếu chính sách hiện có phối hợp thay đổi nhiều entity, mô tả rõ tác dụng của method; không coi đó là predicate chỉ kiểm tra. Lựa chọn giữa rule nhỏ, rule tổ hợp và chính sách phối hợp tùy nhu cầu thực tế.

## 3. Quy tắc phức tạp và phụ thuộc trạng thái

Nếu quy tắc cần nhiều dữ liệu, handler tải dữ liệu cần thiết qua repository port rồi truyền domain object/value vào domain behavior hoặc policy. Không ép mọi quy tắc thành predicate một tham số, và không tạo rule riêng cho mọi dòng if.

Validator không thay thế kiểm tra trạng thái tại thời điểm thực thi hoặc bảo vệ cạnh tranh ghi. Unique constraint, cập nhật có điều kiện, concurrency token hoặc transaction vẫn cần thiết tùy use case. Kiểm tra sớm thành công không chứng minh thao tác ghi sau đó vẫn hợp lệ.

Quyền truy cập endpoint thuộc Presentation; quyền trên tài nguyên và danh tính actor thuộc quá trình điều phối Application. Quy tắc thuần về khả năng thực hiện hành vi có thể thuộc Domain khi phản ánh nghiệp vụ thực tế. Không đưa HttpContext/claims vào entity để kiểm tra quyền.

## 4. Mã lỗi và ánh xạ HTTP


Nếu chọn exception để báo lỗi Domain, dùng DomainException; rule không bắt buộc ném lỗi hoặc mang message. Domain không quyết định HTTP status. Code tập trung tại DomainExceptionCode, message tiếng Việt tập trung tại DomainExceptionMessage, cùng thư mục Domain/Exceptions. BusinessExceptionMessage thuộc Application/Exceptions. Không thêm resolver, parameters hoặc exception con để phân loại.

### Quy ước DomainException

Dùng một DomainException chung, chỉ mang string Code và Message, không chứa danh mục code của mọi module. Trong một project đặt tại Domain/Exceptions; codebase nhiều module có thể chia sẻ type tại phần Domain của Common. Static class DomainExceptionCode nằm ngoài exception, trong feature/module sở hữu quy tắc. Common không tham chiếu ngược module. Không dùng enum, nested Codes hoặc registry toàn cục để kiểm tra code. Khi đổi contract phải rà constructor, consumer và serializer.

Rule có thể trả bool, null có ý nghĩa, kết quả có cấu trúc hoặc dùng guard theo ngữ cảnh. Không bắt buộc rule tự ném exception. Bên gọi xử lý kết quả để chặn thao tác không hợp lệ; nếu chọn cơ chế exception cho lỗi Domain thì dùng DomainException. Lỗi lập trình/hạ tầng không tự động là DomainException. Không để hợp đồng trả kết quả thất bại khiến trạng thái sai được ghi hoặc bị hiểu nhầm là thành công.

Ví dụ constant ngoài exception, tại Domain của feature/module:

```csharp
public static class DomainExceptionCode
{
    public const string VALUE_OUT_OF_RANGE = "FEATURE.VALUE_OUT_OF_RANGE";
}
```

Class exception chung tại Domain/Exceptions/DomainException.cs:

```csharp
namespace YourRootNamespace.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message)
        : this(code, message, null) { }

    public DomainException(string code, string message, Exception? innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }
}
```

Thay namespace thực tế, thêm using System nếu cần. Dùng lại class hiện có nếu tương đương, không tạo trùng. Không thêm HTTP status hoặc framework dependency vào class này.

Code là chuỗi, không còn thuộc tính enum như DOMAIN_EXCEPTION_CODE. Consumer đọc Code; nếu chuyển từ thiết kế enum phải cập nhật các nơi đọc/ghi/serialize, loại bỏ enum và nested Codes cũ trong phạm vi được giao. Giữ overload InnerException khi đã có để không mất thông tin chẩn đoán. Không trả InnerException hoặc nội dung lỗi kỹ thuật cho client. Constant đặc thù nằm trong module sở hữu; module khác không cần tham chiếu module đó chỉ để bắt DomainException chung.

Ví dụ entity:

```csharp
public void ChangeValue(int value)
{
    bool isValid = ValueRules.IsWithinRange(value, ValueLimits.Minimum, ValueLimits.Maximum);
    if (!isValid)
    {
        throw new DomainException(DomainExceptionCode.VALUE_OUT_OF_RANGE, DomainExceptionMessage.ValueOutOfRange);
    }

    Value = value;
}
```

Handler gọi Domain rồi để lỗi truyền lên. Không catch chỉ để throw lại, đổi loại hoặc trả thành công. Chỉ catch khi cần cleanup/rollback thuộc trách nhiệm handler; giữ lỗi gốc và ném lại sau cleanup, không để rollback che mất lỗi chính. Nếu transaction behavior/scope đã xử lý, không thêm catch thừa.

Middleware Presentation bắt DomainException trước nhánh lỗi bất ngờ và mặc định trả 422. Ánh xạ Code bằng constant từ DomainExceptionCode; chỉ map 409 khi code có ý nghĩa xung đột rõ ràng theo contract; không so khớp câu chữ Message để chọn status. Nếu chưa có phân loại, giữ mặc định 422 thay vì đoán nghĩa.

DomainException.Message là thông báo chủ đích an toàn cho client và có thể được trả theo response contract hiện hành. Không đưa dữ liệu nhạy cảm, nội dung exception kỹ thuật hoặc stack trace vào message. Lỗi ngoài dự kiến trả 500 với thông báo chung, log nội bộ. Validator vẫn trả 400 qua pipeline; Domain vẫn bảo vệ invariant ở các đường gọi khác.

Kiểm tra: thao tác hợp lệ cập nhật đúng; rule con và rule tổ hợp xử lý đúng giá trị biên; kết quả thất bại được bên gọi xử lý và không làm đối tượng rơi vào trạng thái sai; lỗi phản hồi đúng hợp đồng đã chọn. Test gọi handler trực tiếp không chứng minh validation pipeline đã chạy.

Presentation ánh xạ lỗi đã biết theo ý nghĩa. Quy ước mặc định nếu ứng dụng chưa có contract khác:

| Ngữ cảnh lỗi | HTTP status |
|---|---|
| Validator chặn input UC | 400 |
| Không đủ quyền trên tài nguyên | 403 |
| Không tìm thấy tài nguyên | 404 |
| Xung đột trạng thái hoặc dữ liệu | 409 |
| Vi phạm nghiệp vụ khác được nhận diện | 422 |
| Lỗi không nhận diện/ngoài dự kiến | 500 |

Validation behavior giữ contract hiện có: BusinessException có Code BAD_REQUEST và Errors kiểu Dictionary<string, List<string>> chứa message theo field. WithErrorCode của FluentValidation không tự trở thành code trong response; không mở rộng cấu trúc field errors để chứa code nếu chưa được yêu cầu. Cùng predicate có thể được validator phát hiện với HTTP 400 hoặc Domain bảo vệ với DomainException Code riêng, HTTP 422. Exception kỹ thuật ngoài dự kiến vẫn là 500; không map mọi ArgumentException thành lỗi client.

## 5. Khi thay đổi nghiệp vụ

1. Xác định thay đổi là invariant miền, chính sách riêng của UC hay hợp đồng transport.
2. Sửa tại nơi sở hữu; nếu là rule thuần dùng chung, sửa predicate tại một nơi.
3. Rà soát consumer để bảo đảm chúng thực sự cùng ý nghĩa. Nếu ngữ cảnh có chính sách khác, tách rule phù hợp thay vì sửa rule chung làm thay đổi mọi UC ngoài ý muốn.
4. Giữ code lỗi ổn định khi ý nghĩa không đổi; thay đổi contract có chủ đích nếu ý nghĩa lỗi đổi. Cập nhật message và HTTP mapping tại các nơi liên quan; không sao chép lại điều kiện nghiệp vụ.
5. Kiểm tra hành vi và giá trị biên: Domain không cho tạo trạng thái sai, validator dùng đúng rule, pipeline chặn handler khi input sai, HTTP trả đúng code/status.

Không viết test chỉ xác nhận tên rule hoặc tên method được gọi. Kiểm tra kết quả quan sát được và các đường gọi có thể bỏ qua validation. Không bắt buộc tạo rule dùng chung nếu invariant chỉ dùng trong Domain và không cần kiểm tra sớm tại validator.
