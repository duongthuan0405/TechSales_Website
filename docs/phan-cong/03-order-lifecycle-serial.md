# Người 3 — Vòng đời Order, serial và phối hợp kho

## Phạm vi sở hữu

BE trong `OrderingModule`: trạng thái đơn, OrderItemUnit, duyệt/giao/hủy và port/adapter Inventory, Payment. Không làm nghiệp vụ AfterSale, hoàn tiền hoặc logic Payment. Người 2 sở hữu nền Order/OrderItem, repository chung, ghép migration và registration Ordering; người 3 cung cấp thay đổi cần thiết, không tạo migration cạnh tranh cùng snapshot.

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

Luồng đã chốt: PENDING → APPROVED → SHIPPING → DELIVERED, không bỏ bước. Staff, Business Admin, Technical Admin được tra cứu và xử lý đơn; khách chỉ thao tác trên đơn bản thân trong phạm vi cho phép.

### 1. Duyệt đơn — ApproveOrder

**Quy tắc nghiệp vụ:**

- Đơn tồn tại, đang PENDING; role xử lý hợp lệ. Chuyển sang APPROVED.
- Số lượng kho đã giữ khi tạo; duyệt không giữ thêm lần nữa. Kiểm tra còn đủ phần giữ cho đơn và gán serial cụ thể khi duyệt.
- Online phải thanh toán thành công mới được duyệt. Tiền mặt/COD chưa trả vẫn được duyệt nếu đơn/kho hợp lệ.
- Không tự coi thiếu phương thức hoặc trạng thái online chưa thành công là đã trả tiền.
- Duyệt lại khi đang APPROVED trả thành công, không gán/giữ kho thêm. Đơn đã chuyển trạng thái khác không được dùng quy tắc gọi lặp để quay ngược hoặc bỏ bước.

**Tích hợp tuần này:** port Payment đọc phương thức/trạng thái qua adapter dữ liệu ảo: online thành công, online chờ/thất bại, tiền mặt/COD chưa trả. Contract Payment do người phụ trách Payment tạo; người 3 tự nối tuần sau. Port Inventory cũng mock.

### 2. Gán serial khi duyệt — AssignOrderItemUnits

**Quy tắc nghiệp vụ:**

- Là bước của duyệt đơn; không bắt buộc API gán riêng. Chưa hỗ trợ đổi serial sau khi gán.
- Serial tồn tại, đúng ProductId, trạng thái phù hợp với phần kho giữ cho đơn; đủ số lượng từng mặt hàng.
- Không trùng trong request hoặc gán đồng thời cho đơn khác đang có hiệu lực.
- Lưu OrderItemUnit trong Ordering; thao tác kho qua port, không sửa trực tiếp Inventory.
- Hủy đơn vẫn giải phóng serial đã giữ, đây không phải feature đổi serial.
- API/output không được lộ serial cho khách trước SHIPPING. Nhân viên/Admin có quyền được xem đã gán; chưa gán trả danh sách rỗng.

**Phối hợp:** người 3 bảo vệ tính đúng của ghi/gán; người 2 thực thi chính sách đọc trong chi tiết đơn. Tận dụng key/mapping hiện có; thay đổi schema cần báo người 2 ghép migration.

### 3. Bắt đầu giao — ShipOrder

**Quy tắc nghiệp vụ:**

- Chỉ role xử lý hợp lệ; APPROVED → SHIPPING, không chuyển trực tiếp từ PENDING.
- Phải đủ serial cho từng mặt hàng trước khi giao.
- Ghi nhận xuất/bán kho khi chuyển SHIPPING qua port Inventory; không đợi DELIVERED mới xuất.
- Giao lại request khi đang SHIPPING trả thành công, không xuất kho hai lần.
- Từ SHIPPING, khách sở hữu được xem serial trong chi tiết đơn; không lộ cho khách khác.

**Tích hợp tuần này:** adapter kho dữ liệu ảo, kiểm tra cả thành công/thất bại; tuần sau xác minh tính nhất quán với Inventory thật.

### 4. Xác nhận giao thành công — DeliverOrder

**Quy tắc nghiệp vụ:**

- Chỉ role xử lý hợp lệ; SHIPPING → DELIVERED.
- Không xuất/bán kho thêm lần nữa vì đã xuất khi SHIPPING. Ghi UpdatedAt theo schema; không tự coi đó là lịch sử trạng thái đầy đủ.
- DELIVERED là căn cứ để contract Order cho biết khách đã nhận sản phẩm và đủ điều kiện mua hàng cho Review.
- Khách sở hữu tiếp tục được xem serial khi DELIVERED.
- Gọi lại khi DELIVERED trả thành công, không lặp side effect; các trạng thái khác phải tuân thủ rule chuyển trạng thái.

### 5. Khách hủy đơn — CancelMyOrder

**Quy tắc nghiệp vụ:**

- Actor từ JWT; chỉ hủy đơn thuộc mình và đang PENDING.
- Khách không hủy APPROVED/SHIPPING/DELIVERED qua feature này.
- Hủy hợp lệ chuyển CANCELLED, giải phóng lượng hàng đã giữ và hoàn lượt voucher đã sử dụng khi tạo đơn.
- Gọi lại khi CANCELLED trả thành công sau kiểm tra ownership, không giải phóng/hoàn lượt lần hai.
- Chi tiết đơn CANCELLED không hiển thị serial cho khách.
- Không cập nhật Payment hoặc thực hiện hoàn tiền.

- Điều kiện hủy của Order áp dụng theo ownership và trạng thái đơn nêu trên, không bổ sung điều kiện chặn riêng theo trạng thái thanh toán trong đợt này. Hủy đơn không đồng nghĩa đã hoàn tiền; không trả kết quả hoặc thay đổi dữ liệu thể hiện đã hoàn tiền.

### 6. Nhân viên hủy đơn — CancelOrderForManagement

**Quy tắc nghiệp vụ:**

- Staff, Business Admin, Technical Admin được hủy đơn tồn tại ở PENDING hoặc APPROVED.
- Không hủy SHIPPING/DELIVERED trong luồng này.
- Chuyển CANCELLED; giải phóng lượng hàng/serial đã giữ và hoàn lượt voucher đã dùng khi tạo.
- Gọi lại khi CANCELLED trả thành công sau kiểm tra quyền, không lặp side effect.
- Điều kiện hủy áp dụng theo role và trạng thái đơn nêu trên, không bổ sung điều kiện chặn riêng theo trạng thái thanh toán trong đợt này. Không tự cập nhật giao dịch hoặc hoàn tiền; không trả kết quả thể hiện đã hoàn tiền.

**Phối hợp:** người 3 gọi bộ port Promotion chung của người 2; yêu cầu người 2 bổ sung nhu cầu hoàn lượt và mock. Không tạo thêm một bộ port/adapter Promotion trùng chức năng.

### 7. Quy tắc xuyên suốt — Request lặp, cạnh tranh và lỗi

**Quy tắc nghiệp vụ:**

- Duyệt/giao/hủy khi đã đúng trạng thái đích trả thành công và không lặp giữ/gán/xuất/giải phóng kho hoặc hoàn lượt voucher.
- Vẫn kiểm tra xác thực, role/ownership; request lặp không cho bỏ bước hoặc quay ngược trạng thái.
- Duyệt/hủy đồng thời chỉ một thao tác hợp lệ thắng; kiểm tra lại trạng thái trong transaction/khóa phù hợp.
- Serial không gán đồng thời sang nhiều đơn. Bảo vệ dữ liệu cục bộ Ordering tuần này; tuần sau xác minh ranh giới Inventory thật.
- Không làm idempotency key chống tạo đơn trùng trong đợt này. Đây là feature khác với chống lặp side effect trên một đơn đã có.
- Transaction cục bộ Order không tự bao phủ Inventory/Promotion; cần xác định bước bù/retry khi tích hợp thật, không nuốt lỗi để báo thành công.
- Không tự thêm nghiệp vụ RETURNED/REPLACED hoặc hoàn tiền, broker/outbox ngoài phạm vi.

## Port, contract và trách nhiệm

| Thành phần gợi ý | Người thực hiện | Phạm vi |
|---|---|---|
| IOrderingInventory / OrderingInventoryAdapter | Người 3 | Giữ số lượng, serial, xuất/bán và giải phóng; tuần này mock |
| IOrderingPayments / OrderingPaymentsAdapter | Người 3 | Đọc phương thức/trạng thái để duyệt; tuần này mock |
| Contract Inventory/Payment tương ứng | Người phụ trách Inventory/Payment | Contract/DTO và implementation provider, không thuộc người 3 |
| IOrderingPromotions / OrderingPromotionsAdapter | Người 2, người 3 nêu nhu cầu hủy | Dùng chung, không tạo bộ thứ hai |
| Contract thao tác Order nếu có consumer cụ thể | Người 3 | Contract/DTO phía Order theo hành vi thực sự cần |

Contract thao tác Order không được là SetStatus tùy ý: mỗi hành vi phải có input/result và rule rõ ràng, điều phối UC nội bộ trong Ordering. Consumer không gọi UC nội bộ trực tiếp. Không tạo contract thao tác chưa có nhu cầu thực tế. Không sửa OrderOwnershipAdapter/PaymentAmountAdapter do bên gọi Payment sở hữu.

## Bàn giao và kiểm tra

- Entity OrderItemUnit class, rule trong Domain/Rules, các UC/API cần thiết, port/adapter mock và DI.
- Người 2 ghép migration/registration Ordering; người 3 tự đăng ký adapter của mình tại IntegrationRegistration và phối hợp tránh conflict.
- Test role, trạng thái sai, online chưa trả bị chặn duyệt, online thành công/tiền mặt chưa trả được duyệt, serial sai/trùng/thiếu, gọi lặp và duyệt/hủy đồng thời trong Ordering.
- Test hủy giải phóng/hoàn lượt đúng một lần bằng mock; chưa báo đã xác minh Inventory/Promotion/Payment thật.
- Phối hợp người 2 test serial khách ở SHIPPING/DELIVERED mới thấy, nhân viên thấy đã gán, chưa gán trả rỗng.
- Build/test phần thay đổi; bàn giao tình huống mock, contract/DTO Order và danh sách tích hợp thật tuần sau. Không tự áp migration lên DB dùng chung.

## Quy ước code

Mỗi use case đủ UCInput, UCOutput, UCHandler, UCInput_Validator; validator qua pipeline. ControllerBase, role tại Authorize(Roles = hằng số); ownership/trạng thái vẫn kiểm tra trong use case. Entity class, rule ở Domain/Rules, repository/I/O ở Infrastructure. List<T>/Task<List<T>>, Items là List<T>; constructor truyền thống, private readonly dependency, if/else/vòng lặp có ngoặc. Message tiếng Việt theo catalog, dùng response/middleware hiện có. Chữ ký/type và cơ chế khóa/phục hồi do nhóm phối hợp kỹ thuật.
