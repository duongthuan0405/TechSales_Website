# Báo cáo tình trạng Backend TechSales

Ngày cập nhật: 25/11/2026. Thời lượng trình bày dự kiến: khoảng 3 phút.

## 1. Tổng quan và các module nghiệp vụ

Backend TechSales được xây dựng bằng ASP.NET Core theo kiến trúc **Modular Monolith**. Mỗi module phụ trách một nhóm nghiệp vụ, tổ chức theo Clean Architecture và sử dụng MediatR để xử lý các use case. Dữ liệu được lưu bằng PostgreSQL thông qua Entity Framework Core.

Hệ thống gồm **10 module nghiệp vụ**:

| Module | Phạm vi nghiệp vụ |
|---|---|
| Auth | Tài khoản, xác thực và phân quyền |
| Catalog | Sản phẩm, hãng và danh mục |
| Inventory | Kho và thiết bị theo số serial |
| Cart | Giỏ hàng |
| Ordering | Đơn hàng |
| Shipping | Địa chỉ giao hàng và phí vận chuyển |
| Promotion | Voucher và khuyến mãi |
| AfterSale | Hoàn trả, bảo hành và sửa chữa |
| PaymentRefund | Thanh toán và hoàn tiền |
| ReviewNotification | Đánh giá sản phẩm, phản hồi và thông báo |

Ngoài ra, **Common** cung cấp cơ chế dùng chung; **Mail** phụ trách gửi email.

## 2. Các module đã hoàn thành chức năng cốt lõi

Hiện có **6 module nghiệp vụ đã triển khai API cho phạm vi cốt lõi**. Đây là mức hoàn thành theo chức năng hiện có, chưa đồng nghĩa toàn bộ hệ thống đã sẵn sàng vận hành thực tế.

- **Auth:** Đã có đăng ký, xác minh email, đăng nhập JWT, quên và đổi mật khẩu; quản lý hồ sơ, nhiều vai trò và vòng đời tài khoản. Khi tạo tài khoản, họ tên và số điện thoại là bắt buộc; số điện thoại duy nhất. Module giải quyết việc xác thực và giới hạn quyền thao tác theo loại người dùng.
- **Catalog:** Đã có quản lý sản phẩm, hãng, danh mục và nhiều hình ảnh. BE thực hiện tìm kiếm, lọc, sắp xếp và phân trang, cung cấp dữ liệu thống nhất cho khách hàng và trang quản trị.
- **Inventory:** Đã có nhập kho bằng serial thủ công hoặc Excel, tra cứu thiết bị, đánh dấu hỏng và khôi phục về kho. Kiểm tra serial trùng và transaction nhập Excel giúp tránh ghi dữ liệu một phần, đồng thời giữ nhất quán trạng thái thiết bị với tồn kho.
- **Cart:** Đã có xem giỏ, thêm sản phẩm, thay đổi số lượng và xóa sản phẩm. Giỏ hàng được lưu theo tài khoản; hệ thống kiểm tra trạng thái kinh doanh trước khi thêm hoặc cập nhật sản phẩm.
- **Shipping:** Đã có quản lý địa chỉ giao hàng, chọn địa chỉ mặc định, tra cứu tỉnh/phường và cấu hình phí vận chuyển theo tỉnh. Module giải quyết việc xác định địa chỉ và phí giao hàng.
- **Promotion:** Đã có tạo, xem, sửa và ngừng voucher; kiểm tra điều kiện cấu hình về thời gian hiệu lực, giá trị giảm và giới hạn sử dụng. Việc áp dụng, ghi nhận sử dụng voucher trong đơn hàng còn cần nối với Ordering.

Về hỗ trợ, **Mail** đã gửi email xác minh và đặt lại mật khẩu; **Common** đã cung cấp xử lý lỗi, chuẩn hóa phản hồi và các cơ chế hạ tầng dùng chung.

## 3. Các module đang triển khai hoặc chưa hoàn tất

- **Ordering:** Đã có cấu trúc lưu trữ, nhưng chưa có đầy đủ API cho luồng tạo, xem, hủy và xử lý đơn hàng.
- **AfterSale:** Đã có cấu trúc lưu trữ, nhưng chưa hoàn thiện API cho quy trình hoàn trả, bảo hành và sửa chữa.
- **ReviewNotification:** Đã có dữ liệu và khả năng tổng hợp điểm đánh giá cho sản phẩm; chưa có đầy đủ API gửi, kiểm duyệt, phản hồi đánh giá và quản lý thông báo.
- **PaymentRefund:** Đã triển khai các API tạo, tra cứu giao dịch, lấy liên kết thanh toán, xử lý phản hồi MoMo/VNPay và xác nhận tiền mặt. Tuy nhiên, phần lấy số tiền và kiểm tra quyền sở hữu đơn hàng/sửa chữa vẫn dùng adapter tạm; chưa hoàn chỉnh nghiệp vụ hoàn tiền và luồng thanh toán xuyên suốt.

## 4. Hướng phát triển tiếp theo

Ưu tiên tiếp theo là hoàn thiện **Ordering**, kết nối đơn hàng với kho, voucher và thanh toán; thay các adapter tạm bằng dữ liệu nghiệp vụ thật. Sau đó triển khai **AfterSale**, hoàn tiền, đánh giá và thông báo. Cần tiếp tục kiểm thử luồng liên module, phân quyền và các tình huống xử lý đồng thời trước khi vận hành thực tế.
