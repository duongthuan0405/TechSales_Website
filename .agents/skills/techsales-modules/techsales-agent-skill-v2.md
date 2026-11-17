# SKILL: TechSales System Architect & Backend Developer Agent (v2)

## 1. Role & System Identity
You are **TechSales Senior .NET Architect & Core Developer Agent**. Your mission is to write, review, refactor, and generate production-grade backend code and database migrations for the **TechSales Website** system (project SE310 - .NET Technology). You must strictly adhere to the system architecture, module boundaries, and complete database schema defined in the TechSales Design System.

---

## 2. Architecture Guidelines

### 2.1 System Topography
- **Client**: Web & Mobile applications communicating via RESTful JSON APIs.
- **Backend API Server**: ASP.NET Core Web API built using a **Modular Monolithic** pattern.
- **Database**: PostgreSQL Relational Database, managed via Entity Framework Core (EF Core) code-first migrations.
- **External Systems**: Payment Gateways (MoMo, VNPay), Third-party Email Services (SMTP / SendGrid).

### 2.2 Modular Monolith Structure (12 Modules)
1. `Common`: Transversal utilities, constants, logging, exception handling, base interfaces, API envelopes.
2. `IdentifyAccess`: Account management, JWT authentication, authorization, user profiles, roles, permissions.
3. `Catalog`: Brands, categories, products, prices, descriptions, product images.
4. `Inventory`: Serialized product unit tracking (`SerialNumber`), warehouse stock counts, reserved stock.
5. `Cart`: User shopping cart and item selection lifecycle.
6. `Ordering`: Order creation, processing lifecycle, item price snapshots, serial assignments, order cancellation.
7. `Shipping`: Administrative units (Province, Ward), shipping addresses, shipping fee policies.
8. `Promotion`: Vouchers, discount calculations, minimum order conditions, voucher usage limits.
9. `AfterSale`: Product returns, exchange requests, repair and warranty tracking, inspection results.
10. `PaymentRefund`: Online/Cash transactions, payment processing, refund handling.
11. `ReviewNotification`: Product ratings/reviews, review replies, system notifications.
12. `EmailSending`: SMTP email delivery through dedicated verification/reset senders. Queue/outbox is not currently implemented.

### 2.3 Clean Architecture (4 Layers per Module)
- **Presentation**: Controllers inherit ControllerBase directly, request DTOs, explicit typed API envelopes, endpoint authorization and OpenAPI documentation.
- **Application**: Each MediatR use case has UCInput, UCOutput, UCHandler and UCInput_Validator. Handlers map Domain entities directly to UCOutput; no intermediate View or duplicate response DTO. Application owns repository/service ports and use-case policies.
- **Domain**: Domain Entities, Value Objects, Domain Events, Enums, Core Business Rules & Invariants.
- **Infrastructure**: EF Core DbContext, Entity Configurations, Repositories, PostgreSQL queries, External Integrations.

All Domain/Entities are classes, never records. Repositories map Domain entities to/from separate DbModels; each table is configured through IEntityTypeConfiguration<TDbModel>. Business list results use List<T>. Use conventional constructors, readonly dependency fields, explicit blocks, and one attribute per bracket pair.

Actual projects live in TechSalesBE/Modules: CommonModule, AuthModule (IdentifyAccess), CatalogModule, InventoryModule, CartModule, OrderingModule, ShippingModule, PromotionModule, AfterSaleModule, PaymentRefundModule, ReviewNotificationModule and MailModule (EmailSending). Each module owns RegisterModule; TechSalesBE.WebAPI composes these registrations and Host/Integration adapters. Common owns shared mechanisms, not a business DbContext.

All business rules belong to each owning module's Domain/Rules, including entity invariants, state transitions, ownership, default selection and use-case-specific policies. Entities own state and call rules; handlers load data and orchestrate I/O/transactions; validators reuse rule predicates. Do not create Application/Rules or duplicate business conditions in entities/handlers. Domain rules do not reference Application or perform I/O.

Place DomainExceptionCode and DomainExceptionMessage together in each module's Domain/Exceptions. Place BusinessExceptionMessage in Application/Exceptions and ResponseMessage in Application/Messages. Keep messages centralized and write all user-facing responses, validation, authentication errors and email text in Vietnamese. Do not translate role identifiers or public error codes. For legacy business error contracts, explicit Application behaviors map selected Domain codes to the existing Business codes/statuses; Domain never references Application message catalogs.

Auth currently uses JWT without persisted sessions or a token blacklist. Logout passes authentication and returns 200; the client deletes its token. UserOtp stores only hashed verification/password-reset OTPs. Token generation belongs to IAccessTokenService and its infrastructure implementation. All existing API authorization currently uses roles at controllers through Authorize(Roles) or RequireRole policies; keep public and authenticated self-service endpoints as designed. Keep permission tables, constants, services and management APIs for future use, but do not enforce HasPermission at endpoints in this phase. Editing permissions must not grant or revoke API access. Role-based account-management business restrictions remain in Domain/Rules and are applied by handlers. Four roles are Customer, Staff, Business Admin and Technical Admin. Business Admin cannot create Technical Admin. Do not restore UserSession from historical migrations.

---

## 3. Module Boundary & Inter-Module Dependency Rules

### Cơ chế phân quyền API hiện tại

| API/chức năng | Quyền truy cập |
|---|---|
| Đăng ký, đăng nhập, xác nhận email, quên/đặt lại mật khẩu | AllowAnonymous |
| Tra cứu tỉnh/phường và phí giao hàng | AllowAnonymous |
| Hồ sơ cá nhân, đổi mật khẩu, đăng xuất, địa chỉ của mình | Authorize: người dùng đã xác thực |
| Xem khách hàng, khóa/mở khóa tài khoản | Staff, Business Admin hoặc Technical Admin |
| Xem nhân sự, tạo/sửa tài khoản, thiết lập phí giao hàng | Business Admin hoặc Technical Admin |
| Quản trị role/permission, phân công role | Technical Admin |

Các role trong cùng danh sách là OR. Kiểm tra ở Controller/action qua Authorize(Roles) hoặc policy RequireRole; không dùng HasPermission ở endpoint hiện tại. Auth dùng AccountRoleNames làm nguồn tên role, RoleAccess tập trung nhóm role của API; UserRoleName tương thích giá trị hiện có. Shipping đăng ký policy RequireRole qua RegisterModule, không reference AuthModule.

JWT dùng RoleClaimType = "role"; Auth xác minh tài khoản hoạt động và nạp lại role hiện tại từ database mỗi request. Đổi role tác động lần gọi kế tiếp với JWT đang có; không thêm session hoặc blacklist. Permission vẫn được lưu và quản trị nhưng thay permission không thay quyền gọi API. Rule Domain vẫn kiểm tra phạm vi tài khoản đích, ví dụ Staff chỉ quản lý Customer, Business Admin không tạo Technical Admin; được vào API không đồng nghĩa được thực hiện mọi thao tác với mọi đối tượng.

1. **Common Module**: Depended on by all other 11 modules. Holds no business domain logic.
2. **Strict Encapsulation**: Business module projects reference CommonModule, not one another. A consumer-owned Application port is implemented by a WebAPI Integration adapter that calls the provider's public interface. Existing public service interfaces may live in Application/Interfaces; do not relocate them solely to enforce a Contracts folder. Handlers do not depend on provider DTOs, repositories, DbContext or Infrastructure. Events are an optional design choice with explicit delivery semantics, not an existing durable queue.
3. **Database Schema Isolation**: Each module logically owns its database tables. Physical SQL Foreign Keys across module boundaries MUST NOT be declared unless within the same module scope. Inter-module references are maintained as logical ID fields (e.g., `Order.UserId` references `IdentityAccess.User.Id` logically).

---

## 4. Comprehensive PostgreSQL Database Schema & Entity Rules

### 4.1 Module-owned Enums Definition

```csharp
// Identity and Access
public enum UserStatus { PENDING, ACTIVE, BLOCKED, INACTIVE }
public enum OtpType { EMAIL_VERIFICATION = 1, RESET_PASSWORD = 2 }

// Catalog
public enum ProductStatus { ACTIVE, DISCONTINUED }

// Inventory
public enum ProductUnitStatus { IN_STOCK, RESERVED, SOLD, DAMAGED }

// Ordering
public enum OrderStatus { PENDING, APPROVED, SHIPPING, DELIVERED, CANCELLED }
public enum OrderItemStatus { SOLD, RETURNED, REPLACED }

// Promotion
public enum VoucherType { FIXED, PERCENT }

// After Sale
public enum ReturnRequestStatus { PENDING, APPROVED, RECEIVED_CHECKING, COMPLETED, CANCELLED }
public enum RepairType { WARRANTY, REPAIR }
public enum RepairProcessingLocation { AT_STORE, AT_MANUFACTURER }
public enum RepairRequestStatus { PENDING, APPROVED, RECEIVED_CHECKING, IN_PROGRESS, COMPLETED, CANCELLED }

// Payment and Refund
public enum TransactionMethodType { ONLINE, CASH }
public enum TransactionStatus { PENDING, SUCCESS, FAILED, CANCELLED }
public enum PaymentType { ORDER, REPAIR }
public enum RefundType { ORDER_CANCELATION, ORDER_RETURN }

// Review and Notification
public enum ReviewStatus { VISIBLE, HIDDEN, DELETED }
public enum ConversationStatus { WAITING, ASSIGNED }
```

---

### 4.2 Module 1: Identity and Access Module (7 Tables)

#### Bảng 1.1: `User`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh duy nhất của người dùng. |
| `Email` | VarChar(255) | NN, UNIQUE | Email đăng nhập, không trùng lặp. |
| `HashedPassword` | VarChar(255) | NN | Chuỗi mật khẩu đã được băm. |
| `Status` | UserStatus | NN | Trạng thái tài khoản (Enum). |
| `FailedLoginAttempts`| Int | NN, DEFAULT 0 | Số lần đăng nhập sai liên tiếp. |
| `LastLoginFailedAt` | TimeStampZ | Nullable | Lần đăng nhập thất bại gần nhất; dùng tính cửa sổ đếm lỗi, không dùng UpdatedAt. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm khởi tạo tài khoản. |
| `UpdatedAt` | TimeStampZ | Nullable | Thời điểm cập nhật gần nhất. |
| `LockedUntil` | TimeStampZ | Nullable | Thời điểm hết hạn tạm khóa tài khoản. |

#### Bảng 1.2: `UserOtp`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh token. |
| `UserId` | UUID | FK, NN | Tham chiếu `User.Id`. |
| `Value` | VarChar(500) | NN | Hash OTP xác nhận email/reset mật khẩu, không lưu JWT hoặc OTP thô. |
| `Type` | OtpType | NN | Mục đích của OTP (Enum). |
| `ExpiredAt` | TimeStampZ | NN | Thời điểm token hết hạn. |
| `UsedAt` | TimeStampZ | Nullable | Thời điểm token được sử dụng. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo token. |
*Ràng buộc đặc biệt*: `UNIQUE(UserId, Type)` — Mỗi user có tối đa một bản ghi theo từng loại, cập nhật/thay mã và trạng thái trên bản ghi đó.

#### Bảng 1.3: `UserProfile`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `UserId` | UUID | PK, FK | Khóa chính đồng thời tham chiếu `User.Id` (Quan hệ 1-1). |
| `FullName` | VarChar(150) | NN | Họ và tên người dùng. |
| `PhoneNumber` | VarChar(20) | NN | Số điện thoại liên hệ. |
| `AvatarUrl` | VarChar(500) | Nullable | Đường dẫn ảnh đại diện. |
| `DateOfBirth` | Date | Nullable | Ngày tháng năm sinh. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo hồ sơ. |
| `UpdatedAt` | TimeStampZ | Nullable | Thời điểm cập nhật hồ sơ. |

#### Bảng 1.4: `Role`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh vai trò. |
| `Name` | VarChar(50) | NN, UNIQUE | Customer, Staff, Business Admin hoặc Technical Admin; dùng hằng số UserRoleName hiện có. |
| `Description` | VarChar(255) | Nullable | Mô tả chi tiết vai trò. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo vai trò. |

#### Bảng 1.5: `Permission`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh quyền hạn. |
| `Code` | VarChar(100) | NN, UNIQUE | Mã quyền duy nhất (ví dụ: `VIEW:ORDER`, `UPDATE:CATALOG`). |
| `Name` | VarChar(100) | NN | Tên hiển thị của quyền. |
| `Module` | VarChar(50) | NN | Tên module quản lý quyền. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo quyền. |
| `UpdatedAt` | TimeStampZ | Nullable | Thời điểm cập nhật quyền. |

#### Bảng 1.6: `RolePermission`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `RoleId` | UUID | PK kép, FK | Tham chiếu `Role.Id`. |
| `PermissionId` | UUID | PK kép, FK | Tham chiếu `Permission.Id`. |
*Khóa chính phức hợp*: `(RoleId, PermissionId)`.

#### Bảng 1.7: `UserRole`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `UserId` | UUID | PK kép, FK | Tham chiếu `User.Id`. |
| `RoleId` | UUID | PK kép, FK | Tham chiếu `Role.Id`. |
*Khóa chính phức hợp*: `(UserId, RoleId)`.

---

### 4.3 Module 2: Catalog Module (4 Tables)

#### Bảng 2.1: `Brand`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh thương hiệu. |
| `Name` | VarChar(100) | NN, UNIQUE | Tên thương hiệu sản xuất. |

#### Bảng 2.2: `Category`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh danh mục sản phẩm. |
| `Name` | VarChar(100) | NN | Tên danh mục. |
| `CreatedAat` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo (Lưu ý tên cột: `CreatedAat`). |

#### Bảng 2.3: `Product`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh sản phẩm. |
| `Name` | VarChar(255) | NN | Tên sản phẩm. |
| `Description` | Text | NN | Mô tả chi tiết sản phẩm. |
| `Price` | BigInt | Nullable | Giá niêm yết của sản phẩm (VND). |
| `Status` | ProductStatus | NN | Trạng thái kinh doanh (`ACTIVE`, `DISCONTINUED`). |
| `BrandId` | UUID | FK, NN | Tham chiếu `Brand.Id`. |
| `CategoryId` | UUID | FK, NN | Tham chiếu `Category.Id`. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo sản phẩm. |
| `UpdatedAt` | TimeStampZ | Nullable | Thời điểm cập nhật thông tin sản phẩm. |

#### Bảng 2.4: `ProductImage`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh hình ảnh. |
| `ProductId` | UUID | FK, NN | Tham chiếu `Product.Id`. |
| `ImageUrl` | VarChar(500) | NN | Đường dẫn lưu trữ hình ảnh. |
| `IsPrimary` | Boolean | DEFAULT false | Cờ đánh dấu ảnh đại diện chính của sản phẩm. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo bản ghi ảnh. |

---

### 4.4 Module 3: Inventory Module (2 Tables)

#### Bảng 3.1: `ProductUnit`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `ProductId` | UUID | PK kép | Mã sản phẩm (FK logic tham chiếu `Catalog.Product.Id`). |
| `SerialNumber` | VarChar(100) | PK kép, NN | Số Serial duy nhất của từng thiết bị vật lý. |
| `Status` | ProductUnitStatus| NN | Trạng thái thiết bị (`IN_STOCK`, `RESERVED`, `SOLD`, `DAMAGED`). |
*Khóa chính phức hợp*: `(ProductId, SerialNumber)`.

#### Bảng 3.2: `Invetory`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `ProductId` | UUID | PK | Định danh sản phẩm (FK logic tham chiếu `Catalog.Product.Id`). |
| `Quantity` | Int | NN | Số lượng tồn kho khả dụng hiện tại. |
| `ReservedQuantity` | Int | DEFAULT 0 | Số lượng hàng đang tạm giữ cho các đơn hàng chưa hoàn tất. |
*Lưu ý tên bảng*: Giữ nguyên tên gốc `Invetory` trong thiết kế CSDL.

---

### 4.5 Module 4: Cart Module (2 Tables)

#### Bảng 4.1: `Cart`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh giỏ hàng. |
| `UserId` | UUID | NN, UNIQUE | Khách hàng sở hữu (FK logic tham chiếu `IdentityAccess.User.Id`). |

#### Bảng 4.2: `CartItem`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `CartId` | UUID | PK kép, FK, NN | Tham chiếu `Cart.Id`. |
| `ProductId` | UUID | PK kép, NN | Sản phẩm trong giỏ (FK logic tham chiếu `Catalog.Product.Id`). |
| `Quantity` | Int | NN, DEFAULT 1 | Số lượng đặt mua. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm thêm vào giỏ hàng. |
| `UpdatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm cập nhật số lượng. |
*Khóa chính phức hợp*: `(CartId, ProductId)`.

---

### 4.6 Module 5: Ordering Module (3 Tables)

#### Bảng 5.1: `Order`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh đơn hàng. |
| `UserId` | UUID | NN | Người đặt hàng (FK logic tham chiếu `IdentityAccess.User.Id`). |
| `Status` | OrderStatus | NN | Trạng thái vòng đời đơn hàng (`PENDING`, `APPROVED`, etc.). |
| `TotalProductAmount` | BigInt | NN, DEFAULT 0 | Tổng tiền sản phẩm trước giảm giá và phí ship. |
| `ShippingFee` | BigInt | NN, DEFAULT 0 | Phí vận chuyển áp dụng. |
| `DiscountAmount` | BigInt | NN, DEFAULT 0 | Tổng số tiền được giảm giá từ voucher/khuyến mãi. |
| `TotalAmount` | BigInt | NN, DEFAULT 0 | Tổng số tiền thanh toán thực tế (`TotalProductAmount + ShippingFee - DiscountAmount`). |
| `ShippingAddressSnapshot` | Text | NN | JSON/String ảnh chụp thông tin địa chỉ giao hàng tại thời điểm đặt. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo đơn hàng. |
| `UpdatedAt` | TimeStampZ | Nullable | Thời điểm cập nhật đơn hàng. |

#### Bảng 5.2: `OrderItem`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `OrderId` | UUID | PK kép, FK | Tham chiếu `Order.Id`. |
| `ProductId` | UUID | PK kép, NN | Sản phẩm được đặt (FK logic tham chiếu `Catalog.Product.Id`). |
| `Price` | BigInt | NN | Đơn giá sản phẩm được chốt tại thời điểm mua hàng. |
| `Quantity` | Int | NN | Số lượng sản phẩm mua. |
*Khóa chính phức hợp*: `(OrderId, ProductId)`.

#### Bảng 5.3: `OrderItemUnit`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `OrderId` | UUID | PK kép, FK kép, NN| Tham gia PK phức hợp & FK phức hợp đến `OrderItem(OrderId, ProductId)`. |
| `ProductId` | UUID | PK kép, FK kép, NN| Tham gia PK phức hợp & FK phức hợp đến `OrderItem(OrderId, ProductId)`. |
| `SerialNumber` | VarChar(100) | PK kép, NN | Serial của thiết bị cụ thể xuất gán cho đơn hàng (`ProductUnit`). |
| `Status` | OrderItemStatus | Nullable | Trạng thái thiết bị trong đơn (`SOLD`, `RETURNED`, `REPLACED`). |
*Khóa chính phức hợp*: `(OrderId, ProductId, SerialNumber)`.
*Khóa ngoại phức hợp*: `(OrderId, ProductId)` -> `OrderItem(OrderId, ProductId)`.

---

### 4.7 Module 6: Shipping Module (4 Tables)

#### Bảng 6.1: `Provice`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Code` | Char(7) | PK | Mã tỉnh/thành phố. (Lưu ý tên bảng: `Provice`). |
| `Name` | VarChar(100) | Nullable | Tên tỉnh/thành phố. |

#### Bảng 6.2: `Ward`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Code` | Char(7) | PK | Mã phường/xã. |
| `Name` | VarChar(100) | Nullable | Tên phường/xã. |
| `ProvinceCode` | Char(7) | FK, NN | Tham chiếu `Provice.Code`. |

#### Bảng 6.3: `ShippingAddress`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh địa chỉ giao hàng. |
| `UserId` | UUID | NN | Người sở hữu địa chỉ (FK logic tham chiếu `IdentityAccess.User.Id`). |
| `WardCode` | Char(7) | FK, NN | Tham chiếu `Ward.Code`. |
| `Detail` | VarChar(255) | NN | Địa chỉ chi tiết (Số nhà, tên đường). |
| `IsDefault` | Boolean | DEFAULT false | Cờ củng cố địa chỉ mặc định của người dùng. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo bản ghi. |
| `UpdateAt` | TimeStampZ | Nullable | Thời điểm cập nhật địa chỉ (Lưu ý tên cột: `UpdateAt`). |
| `DeletedAt` | TimeStampZ | Nullable | Thời điểm xóa mềm địa chỉ. |

#### Bảng 6.4: `ShippingFee`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `ProviceCode` | Char(7) | PK | Mã tỉnh/thành áp dụng (FK logic tham chiếu `Provice.Code`). |
| `Fee` | BigInt | NN, DEFAULT 0 | Phí vận chuyển tiêu chuẩn. |

---

### 4.8 Module 7: Promotion Module (2 Tables)

#### Bảng 7.1: `Voucher`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh voucher. |
| `Code` | VarChar(50) | NN, UNIQUE | Mã giảm giá duy nhất. |
| `Type` | VoucherType | NN | Loại giảm giá (`FIXED`, `PERCENT`). |
| `Value` | BigInt | Nullable | Số tiền giảm hoặc phần trăm giảm. |
| `MaxUsage` | Int | Nullable | Giới hạn tổng số lượt sử dụng voucher. |
| `UsedCount` | Int | DEFAULT 0 | Số lượt đã áp dụng thành công. |
| `MinOrderAmount` | BigInt | Nullable | Điều kiện giá trị đơn hàng tối thiểu để áp dụng. |
| `StartDate` | TimeStampZ | Nullable | Thời điểm voucher bắt đầu có hiệu lực. |
| `EndDate` | TimeStampZ | Nullable | Thời điểm hết hạn voucher. |
| `IsActive` | Boolean | NN, DEFAULT true | Cờ trạng thái bật/tắt kích hoạt. |
| `CreatedAt` | TimeStampZ | Nullable | Thời điểm khởi tạo voucher. |
| `UpdatedAt` | TimeStampZ | DEFAULT now() | Thời điểm cập nhật gần nhất. |

#### Bảng 7.2: `OrderVoucher`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `OrderId` | UUID | PK kép, NN | Đơn hàng áp dụng (FK logic tham chiếu `Ordering.Order.Id`). |
| `VoucherId` | UUID | PK kép | Voucher được áp dụng (FK logic tham chiếu `Voucher.Id`). |
*Khóa chính phức hợp*: `(OrderId, VoucherId)`.

---

### 4.9 Module 8: After Sale Module (7 Tables)

#### Bảng 8.1: `ReturnRequestReason`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh lý do trả hàng. |
| `Name` | VarChar(150) | NN | Tên ngắn gọn của lý do. |
| `Description` | VarChar(500) | Nullable | Mô tả chi tiết lý do trả hàng quy định. |

#### Bảng 8.2: `ReturnRequest`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh yêu cầu trả hàng. |
| `OrderId` | UUID | NN | Đơn hàng gốc (FK logic tham chiếu `Ordering.Order.Id`). |
| `ProductId` | UUID | NN | Sản phẩm yêu cầu trả (FK logic tham chiếu `Catalog.Product.Id`). |
| `Quantity` | Int | NN, DEFAULT 1 | Số lượng yêu cầu đổi trả. |
| `ReasonId` | UUID | FK, NN | Tham chiếu `ReturnRequestReason.Id`. |
| `ReasonDescription` | Text | Nullable | Mô tả sự cố chi tiết từ phía khách hàng. |
| `Status` | ReturnRequestStatus| Nullable | Trạng thái duyệt yêu cầu. |

#### Bảng 8.3: `ReturnRequestImage`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh hình ảnh bằng chứng. |
| `ImageUrl` | VarChar(500) | NN | Đường dẫn lưu ảnh sản phẩm hư hỏng/lỗi. |
| `ReturnRequestId` | UUID | FK, NN | Tham chiếu `ReturnRequest.Id`. |

#### Bảng 8.4: `ReturnResult`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK, FK | Khóa chính đồng thời tham chiếu `ReturnRequest.Id` (Quan hệ 1-1). |
| `ApprovedQuantity` | Int | NN, DEFAULT 0 | Số lượng sản phẩm thực tế được duyệt trả. |
| `ResultCheckingDescription` | Text | Nullable | Ghi chú kết quả thẩm định kỹ thuật. |
| `TotalRefundAmount` | BigInt | NN | Tổng số tiền chốt hoàn lại cho khách hàng. |

#### Bảng 8.5: `ReturnResultProductUnit`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `ReturnResultId` | UUID | PK kép, NN | Tham chiếu `ReturnResult.Id` (FK logic). |
| `ProductId` | UUID | PK kép, NN | Mã sản phẩm được kiểm tra. |
| `SerialNumber` | VarChar(100) | PK kép, NN | Số Serial của thiết bị được kiểm tra. |
| `IsApproved` | Boolean | NN | Cờ chấp nhận cho phép trả thiết bị này hay không. |
| `ResultCheckingDescription` | VarChar(500) | Nullable | Đánh giá tình trạng thực tế của thiết bị có Serial này. |
*Khóa chính phức hợp*: `(ReturnResultId, ProductId, SerialNumber)`.

#### Bảng 8.6: `RepairRequest`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh phiếu yêu cầu sửa chữa/bảo hành. |
| `ProductId` | UUID | NN | Sản phẩm (FK logic tham chiếu `Catalog.Product.Id`). |
| `SerialNumber` | VarChar(100) | NN | Serial thiết bị cần sửa (FK logic tham chiếu `Inventory.ProductUnit`). |
| `CustomerId` | UUID | NN | Khách hàng gửi yêu cầu (FK logic tham chiếu `IdentityAccess.User.Id`). |
| `Status` | RepairRequestStatus| NN, DEFAULT PENDING| Trạng thái xử lý sửa chữa. |
| `ProblemDescription` | Text | Nullable | Mô tả chi tiết lỗi thiết bị gặp phải. |
| `ReceivedDeviceAt` | TimeStampZ | Nullable | Thời điểm cửa hàng nhận thiết bị vật lý. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo phiếu yêu cầu. |

#### Bảng 8.7: `RepairResult`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK, FK | Khóa chính đồng thời tham chiếu `RepairRequest.Id` (Quan hệ 1-1). |
| `RepairType` | RepairType | NN, DEFAULT REPAIR | Loại hình xử lý (`WARRANTY`, `REPAIR`). |
| `ProcessingLocation` | RepairProcessingLocation| Nullable | Nơi thực hiện sửa chữa (`AT_STORE`, `AT_MANUFACTURER`). |
| `CheckingResult` | Text | Nullable | Kết quả chẩn đoán kỹ thuật. |
| `SolutionDescription` | Text | Nullable | Phương án và linh kiện đã thay thế. |
| `TransferredToManufactureAt`| TimeStampZ | Nullable | Ngày chuyển máy sang hãng (nếu có). |
| `ReturnedToStoreAt` | TimeStampZ | Nullable | Ngày hãng/trung tâm trả máy về cửa hàng. |
| `CompletedAt` | TimeStampZ | Nullable | Ngày khách hàng nhận lại máy hoàn tất. |
| `TotalAmount` | BigInt | NN, DEFAULT 0 | Tổng chi phí sửa chữa khách hàng cần thanh toán. |

---

### 4.10 Module 9: Payment and Refund Module (3 Tables)

#### Bảng 9.1: `TransactionMethod`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh phương thức thanh toán. |
| `Name` | VarChar(100) | UNIQUE | Tên phương thức (Ví dụ: MoMo, VNPay, COD Cash). |
| `Type` | TransactionMethodType| Nullable | Phân loại giao dịch (`ONLINE`, `CASH`). |

#### Bảng 9.2: `Payment`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh giao dịch thanh toán. |
| `Type` | PaymentType | Nullable | Phân loại mục đích (`ORDER`, `REPAIR`). |
| `OrderId` | UUID | Nullable | Đơn hàng liên quan (FK logic tham chiếu `Ordering.Order.Id`). |
| `RepairRequestId` | UUID | Nullable | Phiếu sửa chữa liên quan (FK logic tham chiếu `AfterSale.RepairRequest.Id`). |
| `TransactionMethodId` | UUID | FK, NN | Tham chiếu `TransactionMethod.Id`. |
| `Status` | TransactionStatus | NN | Trạng thái giao dịch (`PENDING`, `SUCCESS`, `FAILED`, `CANCELLED`). |
| `Amount` | BigInt | NN, DEFAULT 0 | Số tiền giao dịch. |
| `TransactionRef` | VarChar(255) | Nullable | Mã giao dịch phía Cổng thanh toán (MoMo/VNPay Transaction ID). |
| `FailureReason` | Text | Nullable | Ghi nhận chi tiết nguyên nhân giao dịch thất bại. |
| `CompletedAt` | TimeStampZ | Nullable | Thời điểm giao dịch khớp thành công. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm tạo yêu cầu thanh toán. |
| `UpdatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm cập nhật trạng thái. |

#### Bảng 9.3: `Refund`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh giao dịch hoàn tiền. |
| `Type` | RefundType | Nullable | Nguyên nhân hoàn tiền (`ORDER_CANCELATION`, `ORDER_RETURN`). |
| `ReturnRequestId` | UUID | Nullable | Yêu cầu trả hàng tương ứng (FK logic tham chiếu `AfterSale.ReturnRequest.Id`). |
| `OrderId` | UUID | Nullable | Đơn hàng bị hủy tương ứng (FK logic tham chiếu `Ordering.Order.Id`). |
| `Status` | TransactionStatus | NN, DEFAULT PENDING| Trạng thái tiến trình hoàn tiền. |
| `Amount` | BigInt | NN | Số tiền thực hiện hoàn lại. |
| `RefundTransactionRef` | VarChar(255) | Nullable | Mã tham chiếu giao dịch hoàn tiền phía Cổng thanh toán. |
| `FailureReason` | Text | Nullable | Lý do hoàn tiền không thành công. |
| `CompletedAt` | TimeStampZ | Nullable | Thời điểm tiền hoàn về tài khoản khách hàng. |
| `CreatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm lập lệnh hoàn tiền. |
| `UpdatedAt` | TimeStampZ | NN, DEFAULT now()| Thời điểm cập nhật bản ghi. |

---

### 4.11 Module 10: Review and Notification Module (3 Tables)

#### Bảng 10.1: `Review`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh bài đánh giá. |
| `UserId` | UUID | NN | Khách hàng viết đánh giá (FK logic tham chiếu `IdentityAccess.User.Id`). |
| `ProductId` | UUID | NN | Sản phẩm được đánh giá (FK logic tham chiếu `Catalog.Product.Id`). |
| `Rating` | Int | NN | Điểm đánh giá sao (Giá trị từ 1 đến 5). |
| `Comment` | Text | Nullable | Nội dung nhận xét chi tiết. |
| `Status` | ReviewStatus | Nullable | Trạng thái hiển thị (`VISIBLE`, `HIDDEN`, `DELETED`). |
| `CreatedAt` | TimeStampZ | Nullable | Thời điểm gửi đánh giá. |
| `UpdatedAt` | TimeStampZ | Nullable | Thời điểm sửa bài đánh giá. |

#### Bảng 10.2: `ReviewResponse`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh câu phản hồi. |
| `ReviewId` | UUID | FK, NN | Bài đánh giá gốc tham chiếu `Review.Id`. |
| `UserId` | UUID | NN | Nhân viên/Admin phản hồi (FK logic tham chiếu `IdentityAccess.User.Id`). |
| `Content` | Text | Nullable | Nội dung trả lời nhận xét. |
| `CreatedAt` | TimeStampZ | Nullable | Thời điểm gửi phản hồi. |
| `UpdatedAt` | TimeStampZ | Nullable | Thời điểm chỉnh sửa phản hồi. |

#### Bảng 10.3: `Notification`
| Cột | Kiểu dữ liệu | Constraint | Ghi chú & Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Định danh thông báo. |
| `UserId` | UUID | NN | Người nhận thông báo (FK logic tham chiếu `IdentityAccess.User.Id`). |
| `Title` | VarChar(255) | Nullable | Tiêu đề thông báo. |
| `Content` | Text | Nullable | Nội dung chi tiết thông báo. |
| `IsRead` | Boolean | DEFAULT false | Cờ xác định người dùng đã đọc thông báo chưa. |
| `RefTo` | UUID | Nullable | ID tham chiếu đối tượng nghiệp vụ liên quan (OrderId, ReturnRequestId...). |
| `CreatedAt` | TimeStampZ | Nullable | Thời điểm phát sinh thông báo. |

---

## 5. Entity Framework Core Configuration Guidance for Agents

When generating C# EF Core `IEntityTypeConfiguration<T>` classes:

1. **Enum Conversion**: Always store enums as string values in PostgreSQL to avoid numeric index mismatch:
   ```csharp
   builder.Property(e => e.Status)
          .HasConversion<string>()
          .HasMaxLength(50);
   ```

2. **Composite Keys Mapping Example (`OrderItemUnit`)**:
   ```csharp
   builder.HasKey(oiu => new { oiu.OrderId, oiu.ProductId, oiu.SerialNumber });

   builder.HasOne<OrderItem>()
          .WithMany()
          .HasForeignKey(oiu => new { oiu.OrderId, oiu.ProductId })
          .OnDelete(DeleteBehavior.Cascade);
   ```

3. **Composite Keys Mapping Example (`ProductUnit`)**:
   ```csharp
   builder.HasKey(pu => new { pu.ProductId, pu.SerialNumber });
   ```

4. **Timestamp with Time Zone**: Ensure PostgreSQL `timestamp with time zone` mapping:
   ```csharp
   builder.Property(e => e.CreatedAt)
          .HasColumnType("timestamp with time zone")
          .HasDefaultValueSql("now()");
   ```

5. **Column Name Preservation**: Preserve exact schema column names even if irregular:
   - `Category.CreatedAat` -> `.HasColumnName("CreatedAat")`
   - `Provice.Code` / `ShippingFee.ProviceCode` -> `.HasColumnName("ProviceCode")`
   - `Invetory` -> `.ToTable("Invetory")`
   - `ShippingAddress.UpdateAt` -> `.HasColumnName("UpdateAt")`

---

## 6. Code Generation Workflow Checklist for AI Agents

When implementing any feature in TechSales:
1. **Verify Target Module & Table Schema**: Check table columns, types, and constraints in Section 4.
2. **Implement Domain Entities & Enums**: Place pure entities in `TechSales.Modules.<ModuleName>.Domain`.
3. **Write Fluent API Configurations**: Create `IEntityTypeConfiguration<T>` in `Infrastructure/Persistence/Configurations`.
4. **Implement Repositories & Commands/Queries**: Build MediatR handlers in `Application`.
5. **Enforce Cross-Module Rules**: Ensure no direct SQL joins across separate module DbContexts. Use Application Interfaces or Event Bus.
