
  # Ecommerce Management System

  This is a code bundle for Ecommerce Management System. The original project is available at https://www.figma.com/design/w2rJK1V0VgtXyfvHV5Jqy5/Ecommerce-Management-System.

  ## Running the code

  Run `npm i` to install the dependencies.

  Run `npm run dev` to start the development server.

## Kết nối API thực tế

Ứng dụng đang chạy từ `src/main.tsx` → `src/app/App.tsx`. Các thư mục
`src/pages`, `src/components`, `src/context` cũ không phải cây giao diện chính.

Vite proxy `/api`, `/product-images`, `/avatars` tới `http://localhost:5030`.
Đổi backend bằng biến môi trường `TECHSALES_API_TARGET` khi chạy Vite, hoặc
`VITE_API_BASE_URL` nếu dùng API base URL trực tiếp (backend cần cho phép origin tương ứng).
Tất cả service đã nối sử dụng token hiện tại và xử lý envelope `{ success, message, data }`.

| Chức năng FE | API đang dùng |
|---|---|
| Đăng nhập, đăng ký, xác minh, đổi/quên/reset mật khẩu | `/api/auth/*`, `/api/user/me` |
| Danh sách/chi tiết, thêm/sửa sản phẩm và nhiều ảnh | `/api/products`, `/api/admin/products/*` |
| Giỏ hàng: đọc, thêm đúng số lượng, sửa, xóa | `/api/cart`, `/api/cart/items/{productId}` |
| Xóa toàn bộ giỏ | Gọi DELETE từng item trong snapshot hiện tại; BE chưa có endpoint bulk |
| Danh mục: danh sách, tạo, sửa tên | `/api/categories`, `/api/admin/categories/{id}` |
| Nhập kho từ màn hình sản phẩm | `POST /api/inventory/{productId}/receive` với `serialNumbers` |
| Sổ địa chỉ: CRUD, đặt mặc định | `/api/shipping-address/*` |
| Chọn tỉnh/phường, xem phí giao hàng dự kiến | `/api/shipping/provinces/*`, `/api/shipping/wards/{wardCode}/fee` |
| Voucher: danh sách khách/admin, tạo, ngừng mã | `/api/vouchers`, `/api/admin/vouchers/*` |
| Tài khoản: khách/nhân viên, tạo, sửa email, đổi vai trò, khóa/mở | `/api/admin/users/*`, `/api/admin/rbac/roles`, `/api/admin/rbac/users/{id}/roles` |
| Hồ sơ cá nhân, upload avatar | `PUT /api/user-profile` (multipart), đọc `/api/user/me` |
| Phương thức thanh toán | `GET /api/admin/transaction-methods` |
| Thanh toán lại/đổi phương thức cho đơn có ID thật | `/api/payments/by-ref/{id}`, `/api/payments/{id}/checkout`, `/api/payments/replace` |

Nhập kho cần mỗi máy một serial; không còn thao tác đặt tùy ý tổng tồn kho.
Hồ sơ tên/điện thoại do chủ tài khoản cập nhật; API admin hiện chỉ sửa email và vai trò.
Các trường form không được API hỗ trợ đã bị vô hiệu hóa. Danh mục chưa hỗ trợ xóa/thay thế.
Việc kiểm tra voucher ở checkout chỉ là preview từ dữ liệu thật; chưa thay thế xác nhận từ Order API.

## API có nhưng chưa có giao diện tương ứng

| Nhóm | Chức năng / endpoint |
|---|---|
| Brand | Tạo/sửa thương hiệu: `POST /api/admin/brands`, `PUT /api/admin/brands/{id}`; hiện mới dùng Brand để chọn/lọc sản phẩm |
| Inventory | Import Excel: `POST /api/inventory/import`; đánh dấu serial hỏng: `PATCH /api/inventory/{productId}/units/status` |
| Shipping | Quản trị phí theo tỉnh: `PUT /api/shipping/provinces/{provinceCode}/fee` |
| Voucher | Sửa mã và xem chi tiết quản trị: `PUT/GET /api/admin/vouchers/{id}`; FE hiện có tạo/ngừng mã |
| RBAC | Danh sách permission và gán permission cho role: `GET /api/admin/rbac/permissions`, `PUT /api/admin/rbac/roles/{id}/permissions`; chưa có màn hình quản lý nhiều role/permission |
| Tài khoản | Khóa có thời hạn (`until`), chỉnh ngày sinh; API hỗ trợ nhưng UI hiện chưa có trường tương ứng |
| Payment | Màn hình giao dịch khách/admin, chi tiết giao dịch, xác nhận tiền mặt: `GET /api/payments`, `GET /api/payments/{id}`, `GET /api/admin/payments`, `POST /api/admin/payments/{id}/confirm-cash` |
| Payment | Tạo giao dịch độc lập và giao dịch sửa chữa: `POST /api/payments` (`type: ORDER/REPAIR`); UI hiện chưa có luồng đơn/sửa chữa thật để cấp RefId |

Các callback MoMo/VNPay là endpoint máy chủ/cổng thanh toán, không phải màn hình cần bổ sung.

## FE còn thiếu API tương ứng

Ordering hiện chưa có API tạo đơn, xem/lọc đơn, hủy và cập nhật trạng thái.
Chức năng đặt đơn bị chặn; không tạo đơn giả hoặc xóa giỏ thật sau một giao dịch giả.
Trang checkout chỉ hiển thị số liệu dự kiến từ Catalog/Shipping/Voucher.
Các trang đơn hàng cũ, hoàn trả, sửa chữa, review, notification, dashboard/report vẫn dùng mock
do chưa có API BE tương ứng. Nhật ký hệ thống hiển thị lỗi chưa hỗ trợ thay vì log giả.
Các nút thanh toán trên đơn mẫu không được coi là giao dịch thật; cần Ordering API
để trả ID đơn hợp lệ trước khi dùng các endpoint Payment đã nối.

## Kiểm tra

```powershell
node --test tests/api-services.test.mjs
npm run build
```

Test service giả lập HTTP để kiểm tra route, payload, ánh xạ response, lỗi và token.
Đây không phải test end-to-end với PostgreSQL hoặc cổng thanh toán thật.

