# Ranh giới module và giao tiếp

## 1. Chọn module theo năng lực và ownership

Một module sở hữu ngôn ngữ nghiệp vụ, quy tắc, vòng đời dữ liệu và use case tương đối kết dính. Không chia module chỉ theo Controller/Service/Repository hoặc mỗi bảng một module. Khi hai phần luôn cần thay đổi và commit cùng nhau, xem lại boundary trước khi thêm cơ chế phối hợp phức tạp.

Ghi nhận cho phần việc đang triển khai: module nào là nguồn dữ liệu chính, ai được ghi, contract nào cung cấp cho ngoài, và phụ thuộc nào cần để hoàn thành UC. Không cần tạo tài liệu module rỗng cho mọi thay đổi nhỏ.

## 2. Contract công khai

`Contracts/` của module chứa facade interface, input/result DTO và integration event được công khai có chủ đích. Không xuất entity, DbModel, IQueryable, DbContext, repository, HTTP type hoặc SDK type. Truyền ID/value/snapshot cần thiết, không trả object graph nội bộ.

Ví dụ một contract truy vấn, không mặc định module nào cũng phải có method này:

```csharp
public interface IModuleAApi
{
    Task<CapabilitySnapshot?> GetSnapshotAsync(
        Guid id, CancellationToken cancellationToken = default);
}

public sealed record CapabilitySnapshot(Guid Id, bool IsAvailable);
```

Facade implementation internal nằm ở Application hoặc adapter phù hợp, gọi UC của chính module qua ISender để giữ pipeline nếu đó là boundary đã chọn. Tên UCInput/UCOutput nội bộ không bị lộ trong facade contract. Không gọi handler của module khác trực tiếp.

Mặc định consumer không gọi provider contract từ handler. Consumer sở hữu port tại Application; adapter tại Host/Integration triển khai port và gọi facade provider. Không đặt adapter này tại Infrastructure consumer theo mặc định vì cùng một project, consumer vẫn phải reference provider dù handler đã được cách ly. Chỉ giữ trực tiếp dependency đã có khi nhiệm vụ chưa bao gồm chuyển đổi; không tạo coupling mới theo cách đó.

### Ví dụ A cần khả năng từ B

```text
ModuleA/Application/Ports/IReferenceLookup.cs
ModuleB/Contracts/IModuleBApi.cs
ApplicationHost/Integration/ModuleBReferenceLookup.cs

A.Handler -> A.IReferenceLookup <- Host.Adapter -> B.IModuleBApi
```

```csharp
// ModuleA/Application/Ports/IReferenceLookup.cs
namespace ModuleA.Application.Ports;

public interface IReferenceLookup
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
```

```csharp
// ModuleB/Contracts/IModuleBApi.cs
namespace ModuleB.Contracts;

public interface IModuleBApi
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
```

```csharp
// ApplicationHost/Integration/ModuleBReferenceLookup.cs
using ModuleA.Application.Ports;
using ModuleB.Contracts;

internal sealed class ModuleBReferenceLookup : IReferenceLookup
{
    private readonly IModuleBApi _moduleB;

    public ModuleBReferenceLookup(IModuleBApi moduleB) => _moduleB = moduleB;

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _moduleB.ExistsAsync(id, cancellationToken);
}
```

B đăng ký implementation internal của IModuleBApi trong registration của B. Host đăng ký `services.AddScoped<IReferenceLookup, ModuleBReferenceLookup>()` sau khi đăng ký các module. Handler A inject IReferenceLookup, không biết IModuleBApi và không trả kiểu dữ liệu của B. Port phải public để adapter host triển khai, nhưng không vì thế mà mọi implementation trong A đều public.

Hai method giống nhau chỉ để minh họa. Nếu B thay DTO/chữ ký, adapter map về model do A sở hữu; không sửa port A để mirror mọi thay đổi B. Adapter chuyển đổi lỗi dự kiến theo semantics đã thống nhất, không catch mọi lỗi kỹ thuật rồi trả false. A quyết định ý nghĩa nghiệp vụ của kết quả, B vẫn giữ quy tắc và dữ liệu của B.

Ví dụ này không có UC mới nên không thay thế yêu cầu bộ bốn lớp UC của mỗi module. Không cần HTTP nội bộ, project adapter riêng hoặc đưa port/contract vào Common. Khi A chỉ cần dữ liệu nội bộ mình, dùng port/repository nội bộ bình thường, không thêm adapter liên module thừa.

Contract phải quy định rõ null/not-found, lỗi dự kiến, cancellation và idempotency nếu cần. Không cho caller dựa vào exception implementation riêng của provider. Dùng result contract hoặc lỗi chung đã được thống nhất; tránh phụ thuộc danh mục Domain code của provider chỉ để xử lý trường hợp nghiệp vụ thông thường.

## 3. Không vòng project reference

Contracts nằm cùng project với implementation không tạo isolation bằng project riêng. Nếu A reference B để dùng B.Contracts thì A reference toàn assembly B. Vì vậy mặc định chỉ host/integration reference hai module; A chỉ reference phần dùng chung tối thiểu nếu cần và không reference B. Adapter host không được bypass contract để gọi implementation public vô tình lộ ra.

Nếu B cũng muốn tham chiếu A, không tạo reference ngược hoặc chuyển toàn bộ contracts về Common để né lỗi build. Chọn theo bản chất workflow:

- Thay đổi hướng dependency hoặc facade để một chiều.
- Dùng mô hình mặc định consumer-owned port và host/composition adapter, không chứa chính sách nghiệp vụ.
- Nếu có workflow nghiệp vụ độc lập điều phối nhiều module, đặt trong module điều phối được giao rõ trách nhiệm, không nhét vào Program.cs.
- Dùng sự kiện khi phù hợp với tính nhất quán và không cần kết quả đồng bộ.
- Chỉ tách assembly contract khi thật sự cần và đã nằm trong phạm vi; giải thích đây là ngoại lệ với cấu trúc mặc định một project/module.

Không dùng reflection/service locator để che vòng dependency hoặc bypass public contract. Vòng gọi runtime vẫn là vấn đề dù project graph không có vòng.

Mô hình port/adapter cũng không làm workflow atomic, không tự chặn vòng gọi A -> adapter -> B -> adapter -> A và không xóa coupling ngữ nghĩa. Kiểm tra transaction behavior khi facade B gửi UC trong scope đã có transaction của A; không để hai module vô tình commit cùng tài nguyên hoặc lồng transaction không được hỗ trợ.

## 4. Ownership database

Mỗi module chỉ map và ghi dữ liệu mình sở hữu. Có thể dùng một database với schema/table prefix riêng hoặc nhiều database theo hạ tầng hiện có; không bắt buộc tách database để được gọi là modular.

Khi dùng EF Core, ưu tiên DbContext và migrations của từng module. Host điều phối chạy migration nhưng không sở hữu model nghiệp vụ. Tránh navigation EF xuyên module và DbContext chung chứa mọi bảng. Tính toàn vẹn liên module có thể dựa trên ID và kiểm tra qua contract; nếu giữ FK xuyên schema theo hệ thống hiện có, ghi nhận coupling và ownership migration, không tự xóa constraint đang có.

Read model tổng hợp lấy dữ liệu qua contract hoặc xây projection được phép sở hữu. Không để query tổng hợp lặng lẽ trở thành truy cập repository/database tùy ý của các module khác. Với truy vấn báo cáo cần ngoại lệ, xác định read-only contract/projection và tính nhất quán mong muốn.

## 5. Transaction liên module

Transaction của một UC/module là mặc định. Cùng process hoặc cùng database không có nghĩa các DbContext tự dùng chung transaction. Không giả định gọi facade bằng await làm toàn bộ workflow atomic.

Khi cần phối hợp nhiều module, xác định trước một trong các mô hình:

| Nhu cầu | Hướng xử lý |
|---|---|
| Cần kết quả ngay, chấp nhận các bước có thể thất bại độc lập | Facade đồng bộ, quy định failure/compensation |
| Phản ứng sau commit, chấp nhận trễ | Integration event và consumer idempotent |
| Cần giao nhận tin cậy khi process crash | Outbox tại module phát; retry/deduplication phù hợp ở module nhận |
| Cần atomicity DB thực sự cho nhiều module | Thiết kế transaction coordinator rõ ràng, chỉ khi cùng provider/connection và chấp nhận coupling; xem lại boundary |

Không tự thêm distributed transaction, broker hoặc saga framework. I/O bên ngoài không rollback cùng DB. Không giữ khóa DB trong lúc gọi mạng dài khi có thể tổ chức lại workflow. Với transaction coordinator, phải kiểm chứng các resource thực sự tham gia cùng transaction, không chỉ đặt tên ExecuteAtomically.

## 6. Domain event và integration event

Domain event biểu diễn sự kiện nội bộ miền; integration event là contract module công khai, không gửi nguyên entity. Một bên có thể map domain event sang integration event khi cần.

Publish bằng MediatR chỉ là gọi in-process: không durable queue, không mặc định retry hoặc exactly-once. Publish trước commit có thể khiến module khác phản ứng với thay đổi sẽ rollback; publish sau commit có thể mất event nếu process chết. Dùng outbox khi độ tin cậy này là yêu cầu, không hứa semantics cao hơn implementation.

Consumer có thể nhận trùng: dùng event ID/idempotency và ranh giới ghi phù hợp. Không coi domain exception trong subscriber là rollback tự động dữ liệu module phát. Quy định failure propagation của lời gọi/event, cách log và giới hạn retry theo use case.
