# PaymentRefund — bảo vệ checkout đồng thời

`POST /api/payments/{id}/checkout` xác minh chủ payment, sau đó mở transaction và gọi
`IPaymentLockService.TryLockReferenceAsync(Type, RefId)` trước khi đọc lại trạng thái/URL và gọi gateway.

- Dùng PostgreSQL `pg_try_advisory_xact_lock` với cùng key `payment:{Type}:{RefId}` như tạo/replace payment.
- Nếu key đang bị giữ, checkout trả 409 ngay với thông báo tiếng Việt; không xếp hàng chờ khóa, không gọi gateway.
- Request lấy được khóa phải đọc lại payment. Nếu còn PENDING và đã có URL thì trả URL đã lưu; không tạo checkout mới.
- Nếu chưa có URL, request giữ reference lock qua bước gọi gateway và lưu URL; commit/rollback tự giải phóng khóa.
- Chỉ lấy row lock payment tại bước lưu kết quả để đọc trạng thái mới nhất. Không giữ row lock đó trong lúc gọi gateway, cho phép callback cập nhật payment.
- Khóa nằm ở database, áp dụng giữa các API instance dùng cùng database; không dựa vào FE hoặc khóa RAM.

Không cần migration cho thay đổi này. Transaction và kết nối được giữ trong lúc gọi gateway;
khi timeout/lỗi thì rollback và nhả khóa. Bảo vệ request đồng thời không thay thế đối soát trường hợp
gateway đã tạo giao dịch nhưng BE chưa nhận/lưu được kết quả.

## Kiểm tra

`CheckoutPaymentUrlTests` kiểm tra request thứ hai nhận conflict trong khi request đầu còn chờ gateway,
gateway chỉ được gọi một lần, URL được tái sử dụng sau khi hoàn tất và khóa được nhả khi gateway lỗi.

`PaymentLockPostgresTests` dùng hai kết nối PostgreSQL thật để kiểm tra cùng khóa với create/replace,
thử khóa không chờ và giải phóng khi commit/rollback. Cấu hình `PAYMENTREFUND_TEST_DB_CONNECTION` để chạy;
không có biến này thì test được skip. Test chỉ lấy/nhả advisory lock, không sửa bảng dữ liệu.
