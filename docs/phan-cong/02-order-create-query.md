# Người 2 — Tạo đơn, tra cứu và contract đọc Order

## Phạm vi sở hữu

BE trong `OrderingModule`: nền Order/OrderItem, repository chung, tạo/tra cứu đơn và contract đọc Order. Người 2 ghép migration và registration Ordering. Người 3 sở hữu vòng đời/serial; cùng thống nhất interface trước khi sửa file chung.

## Contract, Port và Adapter

| Thành phần | Công dụng | Người thực hiện | Nơi đặt |
|---|---|---|---|
| Port | Interface mô tả khả năng module gọi cần từ bên ngoài; handler dùng interface này | Người phụ trách bên gọi | Application/Ports của module gọi |
| Contract | Interface và DTO công khai mô tả khả năng module cung cấp; implementation nằm trong module đó | Người phụ trách bên cung cấp | Theo cấu trúc contract hiện có của module cung cấp |
| Adapter | Lớp implement port; khi ghép thật sẽ gọi contract và chuyển đổi dữ liệu/lỗi | Người phụ trách bên gọi | TechSalesBE/TechSalesBE.WebAPI/Integration |

```text
Handler của bên gọi → Port → Adapter tại WebAPI → Contract → Logic bên cung cấp
```

**Host chính là project `TechSalesBE/TechSalesBE.WebAPI`**, khởi động ở `Program.cs`. Adapter đặt trong thư mục `Integration`, đăng ký DI trong `Integration/IntegrationRegistration.cs`. Không có người riêng phụ trách “Host”; mỗi người implement và đăng ký adapter thuộc luồng của mình.

Module nghiệp vụ không reference trực tiếp nhau. Host reference các module để nối port với contract. Adapter không chứa rule nghiệp vụ và không truy cập repository/DbContext của provider. Contract không xuất entity, repository, DbContext hoặc use case nội bộ của module cho bên khác gọi.

Repo chưa thống nhất hậu tố tên. Không quy định một mẫu tên mới bắt buộc; giữ thành phần đã có, không tạo trùng chức năng. Tên mới trong tài liệu chỉ là gợi ý để các bên thống nhất, không phải type đã tồn tại.

### Phạm vi tuần này và tuần sau

- **Tuần này:** làm feature và dữ liệu nội bộ của module; port gọi ra được implement bằng adapter trả dữ liệu ảo. Dữ liệu ảo đặt trong adapter, không đặt trong interface hoặc handler. Ghi rõ TODO và các tình huống mock.
- **Bên cung cấp:** tạo contract và DTO của module mình. Logic nội bộ Review/Order vẫn thuộc người phụ trách feature; không bắt buộc ghép liên module thật tuần này.
- **Contract/implementation module khác** như Auth, Catalog, Inventory, Shipping, Promotion, Cart, Payment do người phụ trách module đó làm. Caller chỉ nêu nhu cầu, không phải viết code provider.
- **Tuần sau:** bên cung cấp hoàn thiện implementation thật; bên gọi tự sửa adapter để nối contract và kiểm tra tích hợp. Provider không cần sửa adapter của caller.
- Không thay adapter thật đang có bằng mock chỉ vì tài liệu mô tả mock cho luồng mới; giữ code hiện có ngoài phạm vi và chưa ghép mới trong đợt này.
- Bàn giao tuần này phải phân biệt “feature nội bộ chạy với mock” và “tích hợp thật đã kiểm chứng”. Test nghiệp vụ nội bộ vẫn cần làm; kiểm tra side effect liên module thật để tuần sau.

### Người phụ trách theo luồng

| Luồng | Port và adapter tại WebAPI | Contract và implementation phía cung cấp |
|---|---|---|
| Review → Order: điều kiện đánh giá | Người 1 | Người 2 |
| Review → Auth: tên/avatar tác giả | Người 1 | Người phụ trách Auth |
| Review → Catalog nếu có nhu cầu kiểm tra độc lập | Người 1 | Người phụ trách Catalog |
| Catalog → Review: điểm và lượt đánh giá | Người phụ trách Catalog, adapter đã có | Người 1 |
| Order → Catalog/Shipping/Cart/Auth khi feature cần | Người 2 | Người phụ trách module tương ứng |
| Order → Promotion: dùng/hoàn voucher | Người 2 sở hữu bộ port/adapter chung; người 3 gọi khi hủy | Người phụ trách Promotion |
| Order → Inventory: giữ hàng, serial, xuất/giải phóng | Người 3 sở hữu bộ port/adapter chung; người 2 gọi khi tạo đơn | Người phụ trách Inventory |
| Order → Payment: phương thức/trạng thái để duyệt | Người 3 | Người phụ trách Payment |
| Payment → Order: chủ sở hữu và số tiền đơn | Người phụ trách Payment | Người 2 |

Không làm FE, Notification, gateway, callback hoặc hoàn tiền trong phân công này. Không sửa PaymentRefundModule hoặc adapter do Payment sở hữu. Giao tiếp Order → Payment chỉ làm port/adapter phía Order.

## Feature và quy tắc nghiệp vụ

### 1. Xem trước checkout — PreviewCheckout

**Input:** danh sách ProductId + Quantity, địa chỉ giao hàng, voucher nếu có; khách lấy từ JWT.

**Quy tắc nghiệp vụ:**

- Danh sách không rỗng, số lượng dương; chuẩn hóa sản phẩm lặp và kiểm tra tràn số.
- Đọc giá/trạng thái hiện tại từ port Catalog; từ chối sản phẩm không tồn tại, ngừng bán hoặc chưa có giá.
- Địa chỉ thuộc khách; phí giao hàng lấy qua port Shipping.
- Voucher qua port Promotion: còn hoạt động, đúng thời gian, còn lượt và đạt điều kiện giá trị đơn.
- BE tính: tiền hàng = tổng đơn giá × số lượng; tổng tiền = tiền hàng + phí vận chuyển − giảm giá. Giảm giá đúng phạm vi chính sách, không làm tổng âm.
- Preview không tạo đơn, giữ kho, dùng lượt voucher hoặc xóa giỏ.

**Tích hợp tuần này:** adapter sản phẩm/địa chỉ/phí/voucher trả dữ liệu ảo; có cả dữ liệu hợp lệ và lỗi điều kiện.

### 2. Tạo đơn — CreateOrder

**Quy tắc nghiệp vụ:**

- Tính lại như preview bằng giá mới nhất tại thời điểm xử lý, kể cả khác preview; không bắt khách xác nhận lại theo phương án đã chọn.
- UserId từ JWT; không tin tổng tiền/trạng thái/thời gian client gửi.
- Lưu OrderItem.Price tại thời điểm mua và snapshot địa chỉ; lưu đủ tiền hàng, phí giao hàng, giảm giá, tổng tiền. Đơn mới PENDING.
- Giữ số lượng kho khi tạo đơn; chưa gán serial. Dùng bộ port Inventory do người 3 sở hữu.
- Ghi nhận lượt dùng voucher khi tạo đơn. Không có voucher thì không gọi sử dụng voucher.
- Sau khi tạo thành công, xóa các mặt hàng đã mua khỏi giỏ, giữ phần còn lại. Tạo thất bại không xóa giỏ.
- Chưa làm idempotency key chống tạo đơn trùng: không yêu cầu FE gửi mã, không lưu mã yêu cầu và không cam kết request tạo đơn gửi lại sẽ trả cùng một đơn.
- Không tạo Payment/gọi gateway trong phạm vi người 2.

**Tích hợp tuần này:** giữ hàng, dùng voucher, xóa giỏ thực hiện qua adapter mock. Có kịch bản thành công/thất bại để test điều phối. Tuần sau kiểm tra phục hồi side effect khi một module thành công nhưng lưu đơn thất bại; transaction Order không tự bao phủ module khác.

### 3. Khách xem danh sách đơn — GetMyOrders

**Quy tắc nghiệp vụ:**

- Chỉ xem đơn của người đang đăng nhập, UserId từ JWT.
- Lọc trạng thái trước count/phân trang; mới nhất trước, thêm Id để ổn định thứ tự.
- Không có đơn trả danh sách rỗng; danh sách không cần serial.

### 4. Khách xem chi tiết đơn — GetMyOrderById

**Quy tắc nghiệp vụ:**

- Đơn phải tồn tại và thuộc actor từ JWT; đổi ID không được xem đơn người khác.
- Trả mặt hàng, giá mua, số lượng, snapshot địa chỉ, các khoản tiền và trạng thái. Giá/địa chỉ đơn cũ không thay đổi theo dữ liệu hiện tại.
- Mỗi mặt hàng có danh sách SerialNumbers lấy từ OrderItemUnit.
- Chỉ trả serial cho khách sở hữu khi đơn SHIPPING hoặc DELIVERED. PENDING/APPROVED/CANCELLED trả danh sách rỗng.
- Chưa gán serial trả rỗng, không null hoặc serial mẫu. Áp dụng tại BE/UCOutput, không chỉ ẩn ở FE.

### 5. Danh sách quản lý đơn — GetOrdersForManagement

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được tra cứu.
- Tìm/lọc theo ID đơn, khách hàng, khoảng thời gian, trạng thái; lọc trước count/phân trang, thứ tự ổn định.
- Không tự thêm mã đơn riêng khi schema hiện chưa có; dùng ID đơn hiện tại.
- Không cần trả serial trong danh sách.

### 6. Chi tiết quản lý đơn — GetOrderByIdForManagement

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được đọc; đơn phải tồn tại.
- Trả dữ liệu đơn, các khoản tiền, snapshot và mặt hàng phục vụ xử lý.
- Nhân viên/Admin có quyền xem các serial đã gán ở mọi trạng thái mà họ được truy cập; chưa gán trả danh sách rỗng.
- Một sản phẩm nhiều chiếc có nhiều serial. Người 3 gán/ghi, người 2 đọc và map output.
- Nếu cần dữ liệu tên/điện thoại hiện tại từ Auth, dùng port/adapter mock, không join bảng Auth. Phân biệt thông tin hiện tại với snapshot lịch sử khi thiết kế response.

### 7. Contract đọc Order cho module khác

**Quy tắc nghiệp vụ:**

| Nhu cầu | Contract gợi ý | Kết quả |
|---|---|---|
| Review kiểm tra khách nhận sản phẩm | IOrderReviewEligibility | Khách có đơn DELIVERED chứa ProductId hay không |
| Payment kiểm tra sở hữu và số tiền | IOrderPaymentQuery | Đơn có tồn tại, chủ sở hữu, TotalAmount và trạng thái cần thiết |
| Module khác đọc snapshot nếu có nhu cầu thật | IOrderQuery | Snapshot đơn/mặt hàng, chỉ phần consumer cần |

- Người 2 tạo contract và DTO phía Order tuần này; không tạo contract chưa có consumer/nhu cầu rõ ràng.
- Contract thật phải phân biệt không tồn tại, không thuộc khách và không đủ điều kiện; không dùng giá thử nghiệm làm implementation thật. Mock chỉ ở adapter của caller.
- Contract không xuất entity/repository/DbContext/UC nội bộ hoặc enum internal. DTO phía port/contract độc lập, adapter chuyển đổi.
- Contract Payment không cần serial. Contract khác nếu trả serial phải có mục đích/quyền rõ ràng, không tạo đường đọc bỏ qua chính sách chi tiết đơn.
- Người phụ trách Review/Payment tự implement adapter bên họ, dùng mock tuần này và ghép tuần sau.

## Port và adapter người 2 chịu trách nhiệm

| Khả năng | Tên port/adapter gợi ý | Provider tạo contract |
|---|---|---|
| Sản phẩm, giá, trạng thái | IOrderingProducts / OrderingProductsAdapter | Catalog |
| Địa chỉ thuộc khách, phí vận chuyển | IOrderingShipping / OrderingShippingAdapter | Shipping |
| Kiểm tra/dùng/hoàn lượt voucher | IOrderingPromotions / OrderingPromotionsAdapter | Promotion |
| Giỏ hàng, xóa mặt hàng đã mua | IOrderingCart / OrderingCartAdapter | Cart |
| Thông tin khách nếu response cần | IOrderingUsers / OrderingUsersAdapter | Auth |

Người 2 tạo port/adapter mock và DI, không làm contract/implementation của các provider trên. Người 3 dùng chung port Promotion khi hủy: báo nhu cầu hoàn lượt, người 2 bổ sung vào bộ port/adapter chung. Với kho, người 2 báo nhu cầu giữ số lượng cho người 3 và dùng port Inventory do người 3 tạo.

## Bàn giao và kiểm tra

- Order/OrderItem class, IOrderRepository và implementation, rule tính tiền/tạo/đọc trong Domain/Rules, API/use case đầy đủ.
- Ghép migration/ModelSnapshot và registration Ordering cho cả hai người; không tự áp migration lên DB dùng chung.
- Test tính tiền, giá thay đổi dùng giá mới, snapshot, quyền sở hữu, lọc/phân trang, giữ hàng lúc tạo, dùng voucher, xóa đúng mặt hàng và thất bại không xóa giỏ.
- Test serial: khách chỉ thấy ở SHIPPING/DELIVERED, khách khác không xem được; quản lý thấy serial đã gán; chưa gán trả rỗng. Rà soát response/contract để không lộ serial qua đường khác.
- Chưa yêu cầu cơ chế/test idempotency key tạo đơn. Quy tắc không lặp side effect duyệt/giao/hủy thuộc người 3 vẫn làm.
- Build/test nội bộ với mock, bàn giao contract/DTO và tình huống mock; tuần sau test tích hợp, transaction/compensation thật.

## Quy ước code

Mỗi use case đủ UCInput, UCOutput, UCHandler, UCInput_Validator; validator chạy qua pipeline. ControllerBase và Authorize(Roles = hằng số) cho quyền API; ownership/rule trong use case. Entity class, rule trong Domain/Rules, I/O trong Infrastructure; danh sách List<T>/Task<List<T>>, Items là List<T>. Constructor truyền thống, private readonly dependency; if/else/vòng lặp có ngoặc. Message tiếng Việt theo catalog, dùng response/middleware hiện có. Các bên tự thống nhất tên/type/chữ ký kỹ thuật.
