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

Controller map request, lấy actor từ danh tính tin cậy và gọi ISender.Send. Handler map Entity trực tiếp thành UCOutput; không thêm Application View hoặc Response DTO lặp lại UCOutput. Controller tạo tường minh ApiSuccessResponse<XXX_UCOutput> theo contract hiện có rồi Ok/Created, không dùng Success(object...). Handler áp dụng quy tắc tài nguyên/nghiệp vụ, lấy dữ liệu qua repository/port, gọi Domain behavior và điều phối transaction/side effect. Không inject DbContext hoặc provider SDK vào handler.

Validation behavior gọi ValidateAsync và gom lỗi theo field, dừng trước handler khi input sai. Không chạy validator dùng chung DbContext song song. Không lặp validation thủ công ở từng endpoint/handler. Handler nội bộ không gọi Send lồng nhau chỉ để dùng lại code; facade của chính module có thể gửi UC như entrypoint có chủ đích.

## 3. Nghiệp vụ và persistence

Rate limit, SMTP, JWT, timeout và giới hạn kỹ thuật thuộc options/constants Infrastructure hoặc Presentation, không mặc định thuộc Domain/Constants. Module bind cấu hình và validate options khi khởi động theo tính năng được bật. Hằng số nghiệp vụ thuộc Domain/Constants; mọi quy tắc/chính sách nghiệp vụ, kể cả riêng UC, thuộc Domain/Rules.

Seed tĩnh đặt trong lớp dữ liệu riêng, dùng ID/thời gian cố định và HasData trong IEntityTypeConfiguration<TDbModel>; DbContext quét configurations. Thay đổi seed được đưa vào migration, không giả định HasData tự chạy mỗi lần khởi động. Bootstrap tài khoản là cơ chế runtime riêng, chỉ bật khi được yêu cầu và cấu hình; không tạo tài khoản mặc định ngoài yêu cầu.

Ưu tiên LINQ cho query thường. Khi thực sự cần khóa dòng PostgreSQL, dùng SQL tham số hóa như FromSqlInterpolated, cùng transaction và thứ tự khóa ổn định. Không ghép input vào SQL, không khóa actor/tất cả dữ liệu mặc định. Projection là kỹ thuật nội bộ repository, không tạo lớp View trung gian cho handler.

Phân biệt quyền gọi API với quy tắc nghiệp vụ dựa trên actor/vai trò. `[Authorize]`/policy/permission ở endpoint kiểm tra quyền truy cập API; hạn chế actor được tạo hoặc quản lý loại tài khoản/tài nguyên nào là quy tắc nghiệp vụ, phải được định nghĩa tại Domain/Rules và được use case áp dụng. Không chuyển quy tắc nghiệp vụ vào Controller/authorization filter rồi bỏ kiểm tra trong use case. Handler không cần truy vấn lại cùng API permission, nhưng phải bảo vệ nghiệp vụ khi được gọi ngoài HTTP.

Các con số giới hạn nghiệp vụ phải được định nghĩa một lần bằng hằng số có tên rõ nghĩa tại `Domain/Constants` của module sở hữu, không hardcode lặp trong entity, rule, validator, handler, service hoặc thông báo. Các nơi kiểm tra cùng quy tắc dùng chung predicate tại Domain/Rules và các hằng số này. Nếu thay đổi giới hạn tác động schema, cập nhật mapping và tạo migration mới; không sửa migration/snapshot lịch sử. Không gom giới hạn nghiệp vụ của mọi module vào Common.

Entity/value object giữ trạng thái và hành vi; invariant và quyết định nghiệp vụ định nghĩa tại Domain/Rules, được entity gọi trước khi thay đổi trạng thái. Validator dùng lại predicate và handler gọi rule với dữ liệu đã tải. Không tạo Application/Rules hoặc sao chép điều kiện nghiệp vụ trong handler/entity. Domain không I/O, không tham chiếu Application và không biết MediatR/FluentValidation/EF/HTTP. Các if điều phối I/O, kiểm tra null kết quả truy vấn, mapping và kỹ thuật không tự động là quy tắc nghiệp vụ.

Repository port thuộc Application, implementation Infrastructure; đầu vào/đầu ra nghiệp vụ là Domain entity, có thể kèm ID/count/metadata phân trang. Không trả Application View, UCOutput, IQueryable, storage model, HTTP type hoặc SDK type qua repository. Tách entity Domain và DbModel; repository map hai chiều, cấu hình mỗi bảng trong IEntityTypeConfiguration<TDbModel>, DbContext dùng ApplyConfigurationsFromAssembly. Với entity detached, ghi lại qua storage model tracked hoặc cơ chế tường minh, không giả định EF tự lưu Domain object. Navigation chỉ nối dữ liệu cùng module; liên module lưu ID logic.

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
    DomainExceptionMessage.InvalidState);
```

Tên code/câu chữ chỉ minh họa. Mỗi module đặt DomainExceptionCode và DomainExceptionMessage cạnh nhau tại Domain/Exceptions; BusinessExceptionMessage tại Application/Exceptions; ResponseMessage tại Application/Messages. Common chỉ chứa thông báo cơ chế chung. Không hardcode message tại throw/validator/controller. Mọi thông báo trả người dùng, gồm lỗi/thành công/validation/JSON/AuthN/email, dùng tiếng Việt; tham số động dùng hàm hoặc property có tên rõ ràng. Không đổi contract exception hoặc tạo resolver tổng quát.

Domain không tham chiếu catalog Application. Rule Domain ném DomainException hoặc cung cấp kết quả để Application phân loại. Với hợp đồng cũ cần giữ Business code/HTTP status, behavior Application có thể ánh xạ các Domain code được liệt kê rõ sang BusinessException và BusinessExceptionMessage. Không ánh xạ đại trà mọi DomainException; cleanup/rollback giữ lỗi gốc. Application có thể alias message Domain để tránh lặp nội dung.

Middleware chung tại Presentation/host nhận BusinessException, map code 400/401/403/404/409; DomainException mặc định 422. Mapping Domain khác nếu cần ở composition/Presentation biết module, không ở shared exception. Lỗi bất ngờ hoặc Business code không nhận diện dùng 500 an toàn theo contract; không lộ stack trace/exception kỹ thuật. Không chọn status bằng Message. Giữ code string trong response khi API yêu cầu.

Middleware không ghi lại body nếu response đã bắt đầu; cancellation do request hủy không biến thành lỗi 500 thông thường. Authentication challenge/model binding không nhất thiết ném exception; nếu cần envelope đồng nhất cho các đường này, cấu hình handler/filter tương ứng trong phạm vi được giao.

## 5. Registration và host

Mỗi module công khai một entrypoint tên RegisterModule, nhận IServiceCollection và IConfiguration. Entry point tự bind DI, options, persistence, facade và endpoint/controller của module. Program gọi trực tiếp RegisterModule của từng module; dùng namespace/class alias khi các entrypoint trùng tên. Không tạo hàm gom AddModulePersistence ở host hoặc đăng ký DbContext module trực tiếp trong Program. Helper persistence nội bộ module vẫn có thể tồn tại và được RegisterModule gọi.

Connection string thuộc cấu hình riêng của từng module: `<MODULE>:DbConnectionString`, tương ứng biến môi trường `<MODULE>__DbConnectionString` (ví dụ AUTH__DbConnectionString). Đọc qua IConfiguration để hỗ trợ environment/user secrets; không hardcode credential, không tự fallback connection string toàn cục. Host không tự mở kết nối hoặc chạy migration khi chỉ đăng ký module. JWT/SMTP/options thực tế được module đăng ký và phải có cấu hình hợp lệ khi tính năng được kích hoạt; không tự bỏ validation để làm host chạy.

Dùng một .env/.env.example chung tại WebAPI, key có prefix module. Runtime hiện tại: CreateBuilder(args) → kiểm tra builder.Environment.IsDevelopment() → DotNetEnv.Env.NoClobber().Load() → Configuration.AddEnvironmentVariables() → AddCommandLine(args) → RegisterModule. Load() mặc định tìm theo working directory; phải chạy host với working directory chứa .env, không giả định nó tự dò tới project. Việc thêm lại provider giúp configuration nhận biến vừa nạp và giữ command-line ưu tiên cao nhất; .env không quyết định môi trường đã được CreateBuilder xác định.

Design-time đi đường riêng: mỗi context có IDesignTimeDbContextFactory, gọi DesignTimeConfiguration.Create(args), dùng EnvironmentConfiguration tìm .env chung qua đường dẫn host/solution rồi tái dùng ConfigureOptions của module. Factory không chạy Program, JWT, SMTP hoặc bootstrap. Chỉ Development đọc .env với NoClobber; thiếu file mặc định được bỏ qua, explicit --env-file thiếu phải báo lỗi. Production/Staging/mặc định Production bỏ qua .env, dùng environment/secret CI/CD. Từ thư mục module chạy `dotnet ef migrations add <Name> -- --environment Development` hoặc `dotnet ef database update -- --environment Development`; không cần --startup-project khi factory và cấu hình design-time đã đầy đủ. Dùng dotnet-ef local qua tool manifest, teammate chạy dotnet tool restore. CI/container có job migration riêng trước triển khai; không tự thêm Database.Migrate() vào startup. Không copy/publish credential; chỉ commit file mẫu.

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
