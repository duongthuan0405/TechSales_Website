
  # Ecommerce Management System

  This is a code bundle for Ecommerce Management System. The original project is available at https://www.figma.com/design/w2rJK1V0VgtXyfvHV5Jqy5/Ecommerce-Management-System.

  ## Running the code

  Run `npm i` to install the dependencies.

  Run `npm run dev` to start the development server.

## Kết nối API thực tế

FE tự làm mới access token khi API cần xác thực trả 401 qua `POST /api/auth/refresh`,
sau đó thử lại request một lần. Refresh token nằm trong cookie HttpOnly do BE quản lý,
không lưu trong localStorage; access token vẫn được lưu ở key `token`.
Các request hết hạn đồng thời dùng chung một lần refresh. Refresh bị từ chối sẽ chuyển về
đăng nhập; lỗi 403 hoặc lỗi mạng/máy chủ không tự đăng xuất. BE phải áp dụng migration
`AddRefreshSessions`; đăng nhập lại một lần sau nâng cấp để nhận refresh cookie.

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
| Danh mục quản trị: phân trang, tìm tên (debounce 500ms), tạo, sửa | `/api/admin/categories?pageNumber=1&pageSize=20&keyword=...`, `/api/admin/categories/{id}` |
| Danh mục public/dropdown customer | `/api/categories` (danh sách đầy đủ, không phân trang) |
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
Đã gỡ dữ liệu mock trong RAM. Đơn hàng, hoàn trả, sửa chữa, review, thông báo và báo cáo chưa có API tương ứng sẽ báo chưa hỗ trợ; không tạo hoặc cập nhật bản ghi giả. Dashboard kỹ thuật chỉ hiển thị số tài khoản lấy từ API thật.
Cần Ordering API trả ID đơn hợp lệ trước khi dùng các endpoint Payment đã nối.

## Kiểm tra

Bộ lọc role ở trang tài khoản và nhân viên gửi xuống `GET /api/admin/users/staff?role=Staff`
(hoặc `Business%20Admin`, `Technical%20Admin`). Query được áp dụng tại database trước count/phân trang.
Business Admin chỉ được lọc Staff ở endpoint này; Technical Admin được lọc cả ba role nội bộ.
Không truyền role để lấy các role nội bộ được phép xem. Role không hợp lệ hoặc ngoài phạm vi trả 403.
Customer dùng endpoint `/api/admin/users/customers` riêng. Kết quả hợp lệ nhưng không có dữ liệu vẫn trả 200.
Tìm kiếm tên/email trên hai màn hình này hiện vẫn lọc ở FE với debounce 500ms.

```powershell
node --test tests/api-services.test.mjs
npm run build
```

Test service giả lập HTTP để kiểm tra route, payload, ánh xạ response, lỗi và token.
Đây không phải test end-to-end với PostgreSQL hoặc cổng thanh toán thật.


## Quản lý sản phẩm, danh mục và thương hiệu

Business Admin và Technical Admin dùng trang /business/products với ba tab products, categories, brands. Tab được lưu ở query tab, ví dụ /business/products?tab=brands, để giữ nguyên khi F5 và hỗ trợ Back/Forward. Đường dẫn /business/categories chuyển về tab danh mục.

Tab thương hiệu dùng GET /api/admin/brands (phân trang, tìm tên), POST /api/admin/brands và PUT /api/admin/brands/{id}. Danh mục và thương hiệu chỉ nhập tên; danh mục không hiển thị mô tả. Sau khi thêm/sửa, danh sách lựa chọn và dữ liệu sản phẩm được làm mới.


## Quản lý kho theo unit

Staff và hai Admin quản lý kho từ nút Kho trên từng dòng sản phẩm. Hộp thoại dùng GET /api/inventory/units, cố định productId và hỗ trợ tìm serial, lọc trạng thái, phân trang tại BE. Nút Nhập kho trong hộp thoại mở form nhập serial thủ công (mỗi dòng một serial). Đánh dấu hỏng có xác nhận qua PATCH /api/inventory/{productId}/units/status. Cho phép chuyển thủ công IN_STOCK → DAMAGED và DAMAGED → IN_STOCK; RESERVED/SOLD thuộc quy trình đơn hàng. Đã bỏ trang/menu kho riêng; URL kho cũ chuyển về trang sản phẩm.


## Danh sách sản phẩm khách hàng

Trang /customer/products dùng một request GET /api/products cho mỗi trang. Query API: pageNumber, pageSize, keyword, categoryId, brandId, minPrice, maxPrice, sortBy (newest/price_asc/price_desc/rating). BE lọc, sắp xếp rồi phân trang; FE không tự sắp xếp hoặc lọc giá. Bộ lọc và phân trang lưu trên URL. Rating là trung bình review VISIBLE từ ReviewNotification qua contract; sản phẩm chưa có review nhận điểm 0. Cần schema ReviewNotification hiện có và khởi động lại BE sau cập nhật; không cần migration mới.


Trang sản phẩm của Staff và hai Admin dùng cùng bộ lọc danh mục, hãng, khoảng giá và thứ tự newest/price_asc/price_desc/rating, bổ sung trạng thái kinh doanh. Các tham số được gửi tới GET /api/admin/products; BE lọc/sắp xếp trước phân trang. Đổi bộ lọc trả về trang 1.


Nút Nhập nhiều sản phẩm ở đầu danh sách sản phẩm upload .xlsx qua POST /api/inventory/import. Đây là nhập unit cho sản phẩm đã có, không tạo sản phẩm mới. File có ProductId/SerialNumber ở sheet đầu, tối đa 5 MB và 5.000 dòng; kết quả hiển thị tổng và từng sản phẩm, lỗi BE được hiển thị theo dòng. Toàn bộ file được xử lý trong một transaction.

Khôi phục unit: PATCH /api/inventory/{productId}/units/status với serialNumber và status IN_STOCK. Chỉ unit DAMAGED được khôi phục; cộng 1 tồn khả dụng, giữ nguyên tồn giữ chỗ. Khóa bản ghi kho và cập nhật unit/tồn trong cùng transaction; yêu cầu lặp bị từ chối để tránh cộng trùng. Trong hộp thoại Kho, unit hỏng có nút Khôi phục về kho với xác nhận.

## Cấu hình phí vận chuyển

Business Admin và Technical Admin mở menu Phí vận chuyển (/business/shipping-fees). Danh sách tỉnh/thành phố hiển thị phí hiện tại qua GET /api/shipping/provinces/{provinceCode}/fee; sửa phí ngay từng dòng và bấm Lưu qua PUT cùng endpoint với payload fee. Có tìm kiếm tên/mã tỉnh không dấu, nút Hủy và nhãn Chưa lưu. Tìm kiếm không làm mất giá trị đang sửa. Phí là số nguyên VND không âm; 0 là miễn phí. Phí áp dụng cho các phường/xã thuộc tỉnh. Khi lưu, cache báo phí checkout được làm mới. Giao diện không tự đặt phí hoặc ghi hàng loạt các tỉnh.

Mọi vai trò có menu Đổi mật khẩu: /customer/change-password, /sales/change-password, /business/change-password, /technical/change-password. Form dùng PUT /api/auth/change-password thật với currentPassword, newPassword và confirmPassword, chỉ báo thành công khi BE chấp nhận.


Ngừng hoạt động tài khoản: hai Admin có nút Ngừng hoạt động kèm xác nhận trong danh sách/hộp thoại khách hàng, nhân viên và danh sách người dùng của Tech Admin. Business Admin chỉ thấy thao tác cho Customer/Staff; Tech Admin cho mọi loại tài khoản khác. Ẩn nút với tài khoản của chính mình và INACTIVE. Backend xác minh lại quyền qua POST /api/admin/users/{id}/deactivate. Tài khoản INACTIVE không được khóa/mở khóa và chưa có thao tác kích hoạt lại.


Quyền quản lý tài khoản cập nhật: Business Admin xem danh sách/chi tiết Customer/Staff, được khóa/mở khóa và ngừng hoạt động nhưng không thêm hoặc sửa thông tin tài khoản. Technical Admin sửa email/vai trò tại Quản lý người dùng. Trạng thái không chỉnh tùy ý trong form: xác minh PENDING→ACTIVE, khóa ACTIVE→BLOCKED, mở khóa BLOCKED→ACTIVE, ngừng hoạt động từ mọi trạng thái→INACTIVE; chưa có kích hoạt lại INACTIVE. Icon khóa là Lock, mở khóa Unlock, ngừng hoạt động UserX. BE tách quyền sửa thông tin (chỉ Tech Admin) và quản lý trạng thái (Business Admin chỉ Customer/Staff).

Hộp thông tin Customer/Staff có avatar kèm fallback chữ viết tắt, tên đậm, badge vai trò/trạng thái theo màu và các trường liên hệ/ngày sinh/ngày tham gia/mã tài khoản. Nút khóa dùng Lock/Unlock, ngừng hoạt động dùng UserX. Thay đổi trạng thái làm mới dữ liệu cả danh sách và hộp thoại.
