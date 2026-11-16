# Review, kiểm chứng và mở rộng

## 1. Trước khi sửa code

Xác định module sở hữu use case và dữ liệu, public contract đang có, hướng project reference, nơi đăng ký handler/validator, middleware host và framework/package hiện hành. Đọc cả implementation và consumer liên quan; tên thư mục/module chưa chứng minh boundary đang đúng.

Giữ contract API/schema/codes hiện hữu trừ khi thay đổi có chủ đích trong nhiệm vụ. Skill không cho phép tự chuyển monolith thành nhiều service hoặc gom toàn repository theo một cấu trúc mới.

## 2. Chuyển đổi có kiểm soát

| Hiện trạng | Hướng đích |
|---|---|
| Một service chứa nhiều UC | Bộ bốn lớp riêng cho mỗi UC có mục tiêu rõ |
| Controller điều phối nghiệp vụ | Handler nội bộ module |
| Gọi repository/DbContext module khác | Facade/contract hoặc projection được sở hữu rõ |
| Request MediatR nội bộ dùng làm API liên module | Contract công khai, facade map vào UC nội bộ |
| Handler A inject contract B, project A reference B | Port do A sở hữu; adapter Host/Integration gọi contract B và map về model A |
| Common chứa toàn bộ entity/rule/domain code | Chuyển về module sở hữu; Common giữ hợp đồng chung tối thiểu |
| Transaction mỗi repository | Một owner transaction UC |
| Event in-memory bị coi là durable | Mô tả đúng semantics; thêm outbox chỉ khi yêu cầu tin cậy |
| Các layer của một module thành nhiều project | Tổ chức lại theo một project/module khi nhiệm vụ yêu cầu |

Di chuyển một luồng hoàn chỉnh và kiểm tra trước khi bỏ đường cũ. Tránh compile trùng file, migration assembly sai hoặc mất controller discovery. Khi gộp/đổi project, kiểm tra package và framework compatibility. Không xóa module/project nằm ngoài phạm vi.

## 3. Kiểm tra phù hợp rủi ro

- Domain: invariant, trạng thái hợp lệ, rule dùng chung; lỗi không để đối tượng rơi vào trạng thái sai.
- UC: thành công, input sai, quyền tài nguyên, dependency lỗi, rollback khi liên quan.
- Pipeline: validator chạy trước handler, không validate/commit nhiều lần; test gọi Handle trực tiếp không chứng minh pipeline.
- Contract: facade có semantics thành công/not-found/lỗi rõ, không lộ implementation type; consumer vẫn tương thích.
- Port/adapter: test handler consumer với fake port, test adapter với fake provider contract để xác minh mapping/cancellation/lỗi; kiểm tra host bind đúng port. Không chỉ kiểm tra rằng có interface. Nếu provider đổi shape nhưng giữ semantics, handler consumer không phải đổi vì type của provider bị rò vào port.
- Persistence: mapping/constraint/concurrency với provider phù hợp; in-memory double không chứng minh transaction SQL thực tế.
- Liên module: lỗi giữa các bước, idempotency, event trùng, side effect sau rollback/process restart nếu độ tin cậy đó là yêu cầu.
- Host: DI resolve, controller/endpoint và middleware hoạt động, code/status/JSON đúng, không lộ lỗi kỹ thuật.

Build các module bị ảnh hưởng, consumer và host. Module chưa nằm trong solution phải build riêng nếu thay đổi liên quan. Không chạy console gửi tin hoặc integration thật chỉ để smoke test. Dùng fake adapter khi không cần I/O thật; không tạo endpoint test public để cố ném lỗi trong sản phẩm.

## 4. Checklist review boundary

- Mỗi module đúng một project CA; không tách project theo layer ngoài quyết định được giao?
- Contract có chủ đích, implementation internal khi có thể, không lộ repository/entity/query type?
- Không reference vòng, không bypass boundary bằng service locator/reflection hoặc gọi UC nội bộ module khác?
- Consumer có tránh project reference trực tiếp provider, handler/port có tránh kiểu dữ liệu của provider, adapter có ở Host/Integration và chỉ chuyển đổi/kết nối không?
- Host chỉ ghép ứng dụng, không chứa nghiệp vụ; Common không thành nơi gom code mọi module?
- Module sở hữu dữ liệu và migration; không ghi xuyên bảng/schema hoặc share DbContext tùy ý?
- Transaction/failure semantics liên module rõ; không nhầm await/Publish với atomicity hoặc durability?
- Đủ bốn lớp UC, discovery đúng, pipeline chung chỉ đăng ký một lần?
- Exception chung string Code, danh mục code Domain ở module sở hữu, HTTP ở Presentation?

Có thể dùng architecture tests kiểm tra reference/type visibility khi có giá trị thực tế; không viết test chỉ so khớp cây thư mục hoặc tên file. Công cụ compile không chặn mọi vi phạm layer trong một assembly nên vẫn phải review signature/import.

## 5. Báo cáo

Nêu rõ module được sửa, contract/dependency mới, dữ liệu/migration thay đổi, cách xử lý lỗi và transaction nếu liên quan, kiểm tra đã chạy và giới hạn chưa xác minh. Phân biệt scaffold với tính năng nối vào host thực sự. Nếu phát hiện boundary sai ngoài phạm vi, ghi nhận ngắn gọn thay vì tự refactor toàn hệ thống.
