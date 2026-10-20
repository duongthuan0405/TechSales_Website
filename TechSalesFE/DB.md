Enum UserStatus {
  PENDING
  ACTIVE
  BLOCKED
  INACTIVE
}

Table User {
  Id UUID [pk]
  Email VarChar [unique, not null]
  HashedPassword VarChar [not null]
  Status UserStatus [not null]
  FailedLoginAttempts Int [default: 0, not null]
  CreatedAt TimeStampZ [not null, default: `now()`]
  UpdatedAt TimeStampZ

  LockedUntil TimeStampZ
}

Enum TokenType {
  EMAIL_VERIFICATION
  RESET_PASSWORD
}

Table UserToken {
  Id UUID [pk]

  UserId UUID [not null, ref: > User.Id]

  Value VarChar [not null]
  Type TokenType [not null]

  ExpiredAt TimeStampZ [not null]
  UsedAt TimeStampZ

  CreatedAt TimeStampZ [not null, default: `now()`]

  indexes {
    (UserId, Type) [unique]
  }
}

Table UserProfile {
  UserId UUID [pk, ref: > User.Id]
  FullName VarChar [not null]
  PhoneNumber VarChar [not null]
  AvatarUrl VarChar 
  DateOfBirth Date 
  CreatedAt TimeStampZ [not null, default: `now()`]
  UpdatedAt TimeStampZ
}

Table Role {
  Id UUID [pk]
  Name VarChar [unique, not null] // ADMIN, STAFF, CUSTOMER
  Description VarChar
  CreatedAt TimeStampZ [not null, default: `now()`]
}

Table Permission {
  Id UUID [pk]
  Code VarChar [unique, not null]   // VIEW:ORDER, CREATE:PRODUCT
  Name VarChar [not null]                      // tên hiển thị
  Module VarChar [not null]                    // ORDER, PRODUCT
  CreatedAt TimeStampZ [not null, default: `now()`]
  UpdatedAt TimeStampZ
}

Table RolePermission {
  RoleId UUID [ref: > Role.Id]
  PermissionId UUID [ref: > Permission.Id]

  indexes {
    (RoleId, PermissionId) [pk]
  }
}

Table UserRole {
  UserId UUID [ref: > User.Id]
  RoleId UUID [ref: > Role.Id]

  indexes {
    (UserId, RoleId) [pk]
  }
}

Table Province {
  Code Char(7) [pk]
  Name VarChar
}

Table Ward {
  Code Char(7) [pk]
  Name VarChar
  ProvinceCode Char(7) [not null, ref: > Province.Code]
}

Table ShippingAddress {
  Id UUID [pk]
  UserId UUID [not null, ref: > User.Id]

  WardCode Char(7) [not null, ref: > Ward.Code]
  Detail VarChar [not null]   // số nhà, đường...

  IsDefault boolean [default: false]

  CreatedAt timestampz [not null, default: `now()`]
  UpdateAt timestampz
  DeletedAt timestampz
}

Table ShippingFee {
  ProviceCode Char(7) [ref: > Province.Code, pk]
  Fee BigInt [not null, default: 0]
}

Enum ProductStatus {
  DISCONTINUED 
  ACTIVE
}

Table Brand {
  Id UUID [pk]
  Name VarChar [not null, unique]
}

Table Product {
  Id UUID [pk]
  Name VarChar [not null]
  Description Text [not null]

  Price BigInt [not null]

  Status ProductStatus [not null] // ACTIVE, DISCONTINUED
  
  BrandId UUID [not null, ref: > Brand.Id]

  CreatedAt TimeStampZ [not null, default: `now()`]
  UpdatedAt TimeStampZ
  CategoryId UUID [not null, ref: > Category.Id]
}

Enum ProductUnitStatus {
  IN_STOCK
  RESERVED
  SOLD
  DAMAGED
}

Table ProductUnit {
  ProductId UUID [not null, ref: > Product.Id]
  SerialNumber VarChar [not null]
  Status ProductUnitStatus [not null]

  indexes {
    (ProductId, SerialNumber) [pk]
  }
}

Table Category {
  Id UUID [pk]
  Name VarChar [not null]

  CreatedAat TimeStampZ [not null, default: `now()`]
}

Table ProductImage {
  Id UUID [pk]
  ProductId UUID [not null, ref: > Product.Id]

  ImageUrl VarChar [not null]
  IsPrimary Boolean [default: false]

  CreatedAt TimeStampz [not null, default: `now()`]
}

Table Inventory {
  ProductId UUID [pk, ref: > Product.Id]

  Quantity Int [not null]         // tồn kho thật
  ReservedQuantity Int [default: 0] // đang giữ cho order
  // Business Rule: available = quantity - reserved_quantity
}

Table Cart {
  Id UUID [pk]
  UserId UUID [not null, unique, ref: > User.Id]
}

Table CartItem {
  CartId UUID [ref: > Cart.Id]
  ProductId UUID [ref: > Product.Id]

  Quantity Int [not null, default: 1]

  CreatedAt TimeStampZ [not null, default: `now()`]
  UpdatedAt TimeStampZ [not null, default: `now()`]

  indexes {
    (CartId, ProductId) [pk]
  }
}

Enum OrderStatus {
  PENDING        
  APPROVED   
  SHIPPING
  DELIVERED
  CANCELLED
}

Table Order {
  Id UUID [pk]
  UserId UUID [not null, ref: > User.Id]

  Status OrderStatus [not null]

  TotalProductAmount BigInt [not null, default: 0]      
  ShippingFee BigInt [not null, default: 0]  
  DiscountAmount BigInt [not null, default: 0]
  TotalAmount BigInt [not null, default: 0]

  ShippingAddressSnapshot text [not null]

  CreatedAt TimeStampZ [not null, default: `now()`]
  UpdatedAt TimeStampZ 
}

Table OrderItem {
  OrderId UUID [ref: > Order.Id]
  ProductId UUID [ref: > Product.Id]

  Price BigInt [not null]   // snapshot giá ngay tại lúc mua
  Quantity Int [not null]

  indexes {
    (OrderId, ProductId) [pk]
  }
}

Enum OrderItemStatus {
  SOLD
  RETURNED
  REPLACED
}

Table OrderItemUnit {
  OrderId UUID [not null]
  ProductId UUID [not null]
  SerialNumber VarChar [not null]
  Status OrderItemStatus

  indexes {
    (OrderId, ProductId, SerialNumber) [pk]
  }
}

Ref: OrderItemUnit.(OrderId, ProductId) > OrderItem.(OrderId, ProductId)
Ref: OrderItemUnit.(ProductId, SerialNumber) > ProductUnit.(ProductId, SerialNumber)

Enum VoucherType {
  FIXED
  PERCENT
}

Table Voucher {
  Id UUID [pk]

  Code VarChar [unique, not null]

  Type VoucherType [not null]
  Value BigInt

  MaxUsage Int 
  UsedCount Int [default: 0]
  
  MinOrderAmount BigInt

  StartDate TimeStampZ
  EndDate TimeStampZ

  IsActive Boolean [not null, default: true]

  CreatedAt TimeStampZ
  UpdatedAt TimeStampZ [default: `now()`]
}

Table OrderVoucher {
  OrderId UUID [ref: > Order.Id]
  VoucherId UUID [ref: > Voucher.Id]

  indexes {
    (OrderId, VoucherId) [pk]
  }
}

Enum ReviewStatus {
  VISIBLE
  HIDDEN
  DELETED
}

Table Review {
  Id UUID [pk]

  UserId UUID [not null, ref: > User.Id]
  ProductId UUID [not null, ref: > Product.Id]

  Rating Int [not null]   // 1 → 5
  Comment Text

  Status ReviewStatus

  CreatedAt TimeStampZ
  UpdatedAt TimeStampZ
}

Table ReviewResponse {
  Id UUID [pk]
  ReviewId UUID [not null, ref: > Review.Id]
  UserId UUID [not null, ref: > User.Id]
  Content Text

  CreatedAt TimeStampZ
  UpdatedAt TimeStampZ
}

Table Notification {
  Id UUID [pk]

  UserId UUID [not null, ref: > User.Id]

  Title VarChar
  Content Text

  IsRead Boolean [default: false]
  RefTo UUID

  CreatedAt TimeStampZ
}

Enum ConversationStatus {
  WAITING
  ASSIGNED
}

Table Conversation {
  Id UUID [pk]

  CustomerId UUID [not null, ref: > User.Id]
  AssignedStaffId UUID [ref: > User.Id]

  Status ConversationStatus [not null, default: 'WAITING']

  Subject VarChar

  CreatedAt TimeStampZ [not null, default: `now()`]
  UpdatedAt TimeStampZ
}

Table Message {
  Id UUID [pk]

  ConversationId UUID [not null, ref: > Conversation.Id]
  SenderId UUID [not null, ref: > User.Id]

  Content Text [not null]

  CreatedAt TimeStampZ [not null, default: `now()`]

  indexes {
    (ConversationId, CreatedAt)
  }
}

Table ReturnRequestReason {
  Id UUID [pk]
  Name VarChar [not null]
  Description VarChar
}

Enum ReturnRequestStatus {
  PENDING
  APPROVED
  RECEIVED_CHECKING
  COMPLETED
  CANCELLED
}

Table ReturnRequest {
  Id UUID [pk]

  OrderId UUID [not null]
  ProductId UUID [not null]

  Quantity Int [not null, default: 1]
  
  ReasonId UUID [not null, ref: > ReturnRequestReason.Id]
  ReasonDescription Text
  Status ReturnRequestStatus
}

Ref: ReturnRequest.(OrderId, ProductId) > OrderItem.(OrderId, ProductId)

Table ReturnRequestImage {
  Id UUID [pk]
  ImageUrl VarChar [not null]

  ReturnRequestId UUID [not null, ref: > ReturnRequest.Id]
}

Table ReturnResult {
  Id UUID [pk, ref: > ReturnRequest.Id]
  ApprovedQuantity Int [not null, default: 0]
  ResultCheckingDescription Text
  TotalRefundAmount BigInt [not null, default: 0]
}

Table ReturnResultProductUnit {
  ReturnResultId UUID [not null, ref: > ReturnResult.Id]
  ProductId UUID [not null]
  SerialNumber VarChar [not null]
  IsApproved bool [not null]
  ResultCheckingDescription VarChar
}

Ref: ReturnResultProductUnit.(ProductId, SerialNumber) > OrderItemUnit.(ProductId, SerialNumber)

Enum RepairType {
  WARRANTY
  REPAIR
}

Enum ProcessingLocation {
  AT_STORE
  AT_MANUFACTURER
}

Enum RepairRequestStatus {
  PENDING
  APPROVED
  RECEIVED_CHECKING
  IN_PROGRESS
  COMPLETED
  CANCELLED
}

Table RepairRequest {
  Id UUID [pk]

  ProductId UUID [not null]
  SerialNumber VarChar [not null]
  CustomerId UUID [not null, ref: > User.Id]

  Status RepairRequestStatus [not null, default: 'PENDING']

  ProblemDescription Text
  
  ReceivedDeviceAt timestamptz
  CreatedAt timestamptz [not null, default: `now()`]
}

Ref: RepairRequest.(ProductId, SerialNumber) > ProductUnit.(ProductId, SerialNumber)

Table RepairResult {
  Id UUID [pk, ref: > RepairRequest.Id]
  RepairType RepairType [not null, default: 'REPAIR']
  ProcessingLocation ProcessingLocation

  CheckingResult Text
  SolutionDescription Text

  TransferredToManufactureAt timestamptz
  ReturnedToStoreAt timestamptz
  CompletedAt timestamptz  

  TotalAmount BigInt [not null, default: 0]
}

Enum TransactionMethodType {
  ONLINE
  CASH
}

Table TransactionMethod {
  Id UUID [pk]
  Name VarChar [unique]
  Type TransactionMethodType
}

Enum TransactionStatus {
  PENDING      // vừa tạo, chưa xử lý
  SUCCESS      // thanh toán thành công
  FAILED       // thất bại
  CANCELLED    // user hủy / timeout
}

Enum PaymentType {
  ORDER
  REPAIR
}

Table Payment {
  Id UUID [pk]

  OrderId UUID [ref: > Order.Id]
  RepairRequestId UUID [ref: > RepairRequest.Id]

  TransactionMethodId UUID [not null, ref: > TransactionMethod.Id]
  Status TransactionStatus [not null]

  Amount BigInt [not null, default: 0]

  TransactionRef varchar   // mã từ cổng thanh toán (nullable nếu COD)

  FailureReason Text
  CompletedAt timestamptz
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz [not null, default: `now()`]
}

Enum RefundType {
  ORDER_CANCELATION
  ORDER_RETURN
}


Table Refund {
  Id UUID [pk]

  ReturnRequestId UUID [ref: > ReturnRequest.Id]
  OrderId UUID [ref: > Order.Id]
  Type RefundType

  Status TransactionStatus [not null, default: 'PENDING']
  Amount BigInt [not null]

  RefundTransactionRef VarChar

  FailureReason Text
  CompletedAt timestamptz
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz [not null, default: `now()`]
}
