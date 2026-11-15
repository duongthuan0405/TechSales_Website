# Chuyển đổi code hiện hữu và review kiến trúc

## 1. Khảo sát

Xác định project đích, framework/package, composition root, API contract, schema và side effect. Đọc luồng từ endpoint đến persistence để hiểu hành vi. Code hiện hữu là nguồn tham khảo, không tự động là đặc tả đúng của mọi quy tắc.

Không suy diễn áp dụng skill thành yêu cầu xóa module, gộp mọi project, đổi database hoặc refactor toàn hệ thống. Chuyển đúng phần được giao và nêu rõ phần cấu trúc cũ còn ngoài phạm vi.

## 2. Bản đồ chuyển đổi

| Thành phần hiện hữu | Trách nhiệm đích |
|---|---|
| Service chứa nhiều thao tác nghiệp vụ | Tách theo UC, mỗi UC đủ bốn lớp |
| Params/command/query | XXX_UCInput triển khai IRequest của output tương ứng |
| Tuple/kết quả service | XXX_UCOutput có field rõ nghĩa |
| Điều phối trong controller | XXX_UCHandler |
| Validation input rải rác | XXX_UCInput_Validator qua pipeline |
| Invariant trong service/repository | Domain behavior phù hợp |
| Query/SDK trong handler/controller | Port Application và adapter Infrastructure |
| Persistence entity dùng xuyên các lớp | Tách storage model, domain type và DTO |
| Commit từng repository | Một owner transaction cho UC |
| Enum exception code hoặc nested Codes | String Code, static class constant nằm ngoài exception |
| Danh mục Domain code của mọi module trong Common | Chuyển code về feature/module sở hữu; giữ type DomainException chung |

Một method cũ chưa chắc là một UC, có thể chỉ là helper. Xác định UC bằng mục tiêu của actor và kết quả nghiệp vụ. Không tạo UC cho từng câu lệnh database hoặc gộp mục tiêu độc lập chỉ để giảm file.

## 3. Chuyển từng luồng

1. Ghi nhận input/output, quyền, lỗi, thay đổi dữ liệu và side effect cần giữ.
2. Định nghĩa bộ bốn lớp UC, port và Domain behavior.
3. Chuyển logic và giữ mapping tương thích; không đổi JSON/cột chỉ vì đổi tên C#.
4. Nối controller qua ISender; đăng ký handler, validator, behavior và adapter.
5. Kiểm tra thành công và nhánh lỗi trước khi bỏ đường chạy cũ.
6. Chỉ xóa code/project trong phạm vi được giao, kiểm tra reference và tránh compile trùng file khi di chuyển.

Tìm stub, giá trị thành công cố định, quyền bị bỏ qua, transaction chỉ có try/catch nhưng không atomic, hoặc adapter chưa nối DI. Class tồn tại không chứng minh tính năng hoàn tất.

## 4. Review ranh giới

- Domain có phụ thuộc I/O, HTTP, ORM, MediatR, FluentValidation hoặc SDK không?
- Application có import Infrastructure/Presentation hoặc lộ storage/query/HTTP type trong signature không?
- Mỗi UC có đủ bốn lớp đúng tên và đúng một handler không?
- Validator có được đăng ký, chạy trước handler và trả field errors không?
- Controller có query database, quyết định nghiệp vụ hoặc commit không?
- Transaction có đúng một owner, rollback đúng và không giả định rollback được I/O từ xa không?
- Mapping có giới hạn dữ liệu client được ghi và dữ liệu được trả không?
- Quyền trên tài nguyên và concurrency có được thực thi thay vì chỉ check trước ghi không?
- BusinessException và DomainException có dùng string Code, constant bên ngoài và độc lập về kế thừa không? BusinessException có giữ Errors/AddError không?
- Code Domain đặc thù có nằm ở feature/module sở hữu thay vì Common không? Consumer đã bỏ enum/nested Codes/thuộc tính enum cũ và đọc Code chưa?
- Middleware có map bằng code constant, giữ HTTP ở Presentation, không đoán theo Message và không đổi JSON string thành số ngoài ý muốn không?

Kết hợp đọc mã, tìm import/signature và kiểm tra hành vi. Không kết luận chỉ dựa cây thư mục hoặc thêm test chỉ xác nhận tên file.

## 5. Báo cáo

Nêu UC được triển khai/chuyển đổi, behavior mới, contract được giữ/đổi, kiểm tra đã chạy và giới hạn chưa xác minh. Phân biệt scaffold với tính năng hoàn tất; build thành công chưa chứng minh toàn bộ hành vi đúng.
