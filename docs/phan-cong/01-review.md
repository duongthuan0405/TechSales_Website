# Người 1 — Review và phản hồi đánh giá

## Phạm vi sở hữu

Triển khai BE cho Review và ReviewResponse trong `ReviewNotificationModule`: API, entity/rule, persistence, use case, contract cung cấp và adapter gọi ra. Không làm Notification. Không thêm OrderId chỉ để chống trùng đánh giá.

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

### 1. Tạo đánh giá — CreateReview

**Input:** ProductId, Rating, Comment; tác giả lấy từ JWT.

**Quy tắc nghiệp vụ:**

- Chỉ khách có đơn DELIVERED chứa sản phẩm mới được đánh giá; kiểm tra qua port Order.
- Mỗi người chỉ tạo một đánh giá cho một sản phẩm, kể cả mua lại. HIDDEN/DELETED vẫn chiếm cặp UserId + ProductId, không cho tạo lại.
- Rating là số nguyên từ 1 đến 5. Comment không bắt buộc, tối đa 2.000 ký tự.
- Đánh giá mới VISIBLE; thời gian do server cấp. Không tin UserId, Status, CreatedAt/UpdatedAt client gửi.
- Kiểm tra trùng trong nghiệp vụ và thêm unique constraint (UserId, ProductId). Hai request đồng thời chỉ tạo được một bản ghi; lỗi trùng phải được chuyển thành thông báo tiếng Việt.

**Tích hợp tuần này:** người 1 tạo port/adapter Order trả dữ liệu ảo, có tình huống đủ và không đủ điều kiện. Nếu Order đã chứng minh khách nhận đúng sản phẩm thì không cần gọi Catalog riêng chỉ để kiểm tra tồn tại.

### 2. Sửa đánh giá bản thân — UpdateMyReview

**Input:** ReviewId, Rating, Comment; tác giả lấy từ JWT.

**Quy tắc nghiệp vụ:**

- Khách được sửa điểm/comment của đánh giá bản thân, không sửa của người khác.
- Rating 1–5; comment không bắt buộc, tối đa 2.000 ký tự.
- Không đổi UserId/ProductId, không tạo bản ghi mới, giữ CreatedAt và cập nhật UpdatedAt.
- Không nhận Status từ client; sửa nội dung không tự làm hiện lại đánh giá bị kiểm duyệt.
- Sau khi sửa đánh giá VISIBLE, thống kê phải phản ánh điểm mới.

- Phạm vi UpdateMyReview trong đợt này: chỉ sửa đánh giá VISIBLE; HIDDEN và DELETED không cho sửa. Không triển khai luồng sửa nội dung đánh giá bị kiểm duyệt.

### 3. Khách đọc đánh giá sản phẩm — GetProductReviews

**Input:** ProductId, phân trang; người xem có thể chưa đăng nhập.

**Quy tắc nghiệp vụ:**

- API public chỉ lấy VISIBLE. HIDDEN/DELETED không xuất hiện, kể cả của bản thân.
- Đã đăng nhập: đánh giá bản thân lên đầu; các đánh giá còn lại mới nhất trước. Chưa đăng nhập: tất cả mới nhất trước.
- Dùng CreatedAt giảm dần và Id làm tiêu chí phụ ổn định; lọc và sắp xếp trước phân trang. TotalCount tính trên tập đã lọc.
- Trả số sao, comment, thời gian, thông tin tác giả và phản hồi. Không trả email/điện thoại của tác giả.
- Không có dữ liệu trả danh sách rỗng. UserId người xem lấy từ JWT tùy chọn, không nhận ID tùy ý để giả làm người khác.

**Tích hợp tuần này:** người 1 tạo port/adapter Auth trả tên/avatar ảo; yêu cầu theo danh sách UserId để tránh một lần gọi cho mỗi đánh giá. Người phụ trách Auth tạo contract/implementation; hiện chưa có contract batch phù hợp cho nhu cầu này.

### 4. Quản lý đánh giá theo sản phẩm — GetReviewsForManagement

**Input:** ProductId, trạng thái, số sao nếu dùng, phân trang.

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được xem quản lý.
- Trang quản lý theo sản phẩm có bộ lọc trạng thái. Không tự áp bộ lọc VISIBLE của public; người quản lý được xem cả HIDDEN/DELETED.
- Lọc trước count/phân trang, thứ tự ổn định.
- Trả đủ nội dung, tác giả, trạng thái và phản hồi để kiểm duyệt.
- Không làm API GetReviewByIdForManagement riêng trong đợt này.

### 5. Ẩn đánh giá — HideReview

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được thực hiện; Review phải tồn tại.
- VISIBLE → HIDDEN; đã HIDDEN thì trả thành công, không thay đổi dữ liệu.
- Không ẩn DELETED hoặc dùng thao tác này để khôi phục đánh giá.
- Sau khi ẩn không hiển thị public, không tính vào thống kê; dữ liệu và unique constraint vẫn giữ.

### 6. Hiện lại đánh giá — ShowReview

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được thực hiện; Review phải tồn tại.
- HIDDEN → VISIBLE; đã VISIBLE thì trả thành công, không thay đổi dữ liệu.
- Không khôi phục DELETED.
- Sau khi hiện lại được hiển thị public và tính vào thống kê.

### 7. Xóa mềm đánh giá — DeleteReview

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được thực hiện.
- VISIBLE hoặc HIDDEN → DELETED; đã DELETED thì trả thành công, không đổi dữ liệu.
- Không xóa vật lý, không khôi phục DELETED, không giải phóng cặp UserId + ProductId.
- Không hiển thị public, không tính thống kê; giữ phản hồi cũ nhưng không thêm phản hồi mới.

### 8. Phản hồi đánh giá — CreateReviewResponse

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được phản hồi; tác giả lấy từ JWT.
- Review tồn tại và không DELETED; nội dung không rỗng, tối đa 2.000 ký tự.
- Một đánh giá được có nhiều phản hồi, mỗi phản hồi có tác giả/thời gian riêng.
- Chưa giao thêm feature sửa/xóa phản hồi trong đợt này.

### 9. Cung cấp thống kê điểm cho Catalog — IProductRatings

**Quy tắc nghiệp vụ:**

- Tái sử dụng contract `ReviewNotificationModule.Contracts.IProductRatings` và ProductRatingSnapshot đang có.
- Theo từng ProductId, chỉ tính VISIBLE: AverageRating là trung bình Rating, ReviewCount là số đánh giá; phản hồi không tính thành đánh giá.
- Tạo/sửa/ẩn/hiện/xóa phải phản ánh vào kết quả tổng hợp.
- Giữ cách truy vấn hiện có: trả nhóm có đánh giá VISIBLE; consumer dùng điểm/lượt bằng 0 khi không có dòng. Không tự làm tròn trong truy vấn tổng hợp.

**Phân công:** người 1 cung cấp contract/DTO và nghiệp vụ Review. ProductRatingsAdapter thuộc bên gọi Catalog; người phụ trách Catalog tự nối tuần sau, người 1 không sửa adapter thay họ.

## Thành phần và bàn giao

| Thành phần gợi ý | Người làm | Vị trí |
|---|---|---|
| IReviewOrders | Người 1 | ReviewNotificationModule/Application/Ports |
| ReviewOrdersAdapter, DI | Người 1 | TechSalesBE.WebAPI/Integration |
| Contract kiểm tra khách đã nhận sản phẩm | Người 2 | OrderingModule |
| IReviewUsers | Người 1 | ReviewNotificationModule/Application/Ports |
| ReviewUsersAdapter, DI | Người 1 | TechSalesBE.WebAPI/Integration |
| Contract đọc tên/avatar theo danh sách UserId | Người phụ trách Auth | AuthModule |
| IProductRatings/ProductRatingSnapshot | Người 1 | ReviewNotificationModule/Contracts, đã có |

- Entity Review/ReviewResponse dùng class; rule trong Domain/Rules; repository và implementation persistence thuộc Review.
- Migration unique constraint kèm Designer/ModelSnapshot; kiểm tra dữ liệu trùng trước migration, không tự xóa dữ liệu hoặc áp migration lên DB dùng chung.
- Test: điểm/comment sai, trùng/đồng thời, chưa nhận hàng, sửa của người khác, phân quyền, lọc trạng thái quản lý, sắp xếp trước phân trang và thống kê VISIBLE.
- Build/test phần thay đổi; bàn giao rõ feature nội bộ, contract/DTO, mock scenario và phần ghép thật tuần sau.

## Quy ước code

Mỗi use case đủ UCInput, UCOutput, UCHandler, UCInput_Validator; validator chạy qua pipeline. Controller kế thừa ControllerBase, role ở Authorize(Roles = hằng số); ownership/rule nghiệp vụ vẫn áp dụng trong use case. Không thêm policy permission trong giai đoạn này. List<T>/Task<List<T>>, Items phân trang là List<T>; entity không dùng record; constructor class truyền thống, dependency private readonly, if/else/vòng lặp có ngoặc. Message tiếng Việt qua catalog module, dùng middleware/response hiện có. Các bên tự thống nhất chữ ký/type DTO, không bắt người dùng chốt chi tiết kỹ thuật.
