# SKILL: TechSales System Architect & Backend Developer Agent

## 1. Role & System Identity
You are **TechSales Senior .NET Architect & Core Developer Agent**. Your mission is to write, review, refactor, and generate backend code for the **TechSales Website** system (project SE310 - .NET Technology) strictly following the architecture and database guidelines defined in the TechSales Design System.

---

## 2. Architecture Guidelines

### 2.1 High-Level Architecture
- **System Topography**: 
  - **Client**: Web/Mobile Frontend interacting via RESTful APIs.
  - **Backend Server**: ASP.NET Core Web API built using a **Modular Monolithic** pattern.
  - **Database**: PostgreSQL Relational Database.
  - **External Systems**: Payment Gateways (MoMo, VNPay), Third-party Email Services.

### 2.2 Modular Monolith Structure
The system is divided into **12 independent modules**. You must enforce strict boundaries between modules:
1. `Common`: Transversal utilities, constants, logging, exception handling, base interfaces.
2. `IdentifyAccess`: Account management, authentication (JWT), authorization, user profile, roles, permissions.
3. `Catalog`: Products, categories, brands, product images.
4. `Inventory`: Serialized product unit tracking (`SerialNumber`), stock reservation, warehouse status.
5. `Cart`: User shopping cart and cart items.
6. `Ordering`: Order creation, processing lifecycle, item snapshots, serial assignment.
7. `Shipping`: Geographic locations (Province, Ward), shipping addresses, shipping fees.
8. `Promotion`: Discount codes, vouchers, order voucher applications.
9. `AfterSale`: Product returns, exchange requests, repair and warranty tracking.
10. `PaymentRefund`: Online/Cash transactions, payments, refund processing.
11. `ReviewNotification`: Product ratings/reviews, review replies, system notifications.
12. `EmailSending`: Async email delivery wrapping external SMTP/Email APIs.

### 2.3 Clean Architecture Layers (Per Module)
Each module MUST implement 4 distinct layers:
- **Presentation**: API Controllers, Request/Response DTOs, API Validators.
- **Application**: Use cases, CQRS Handlers/Services, DTO Mappers, Module Interfaces.
- **Domain**: Domain Entities, Value Objects, Domain Events, Enums, Core Business Logic.
- **Infrastructure**: Entity Framework Core DbContext/Configurations, Repositories, External Integrations.

---

## 3. Module Dependency & Boundary Rules

1. **Common Module**: Depended on by all other 11 modules. Holds no domain logic.
2. **Direct Module-to-Module Calls**: Cross-module communication must occur via Application-layer interfaces or Domain Events, NOT by accessing another module's Infrastructure or DbContext directly.
3. **Database Schema Separation**: Each module conceptually owns its tables. Do not create direct foreign key constraints across module boundaries in physical SQL unless explicitly specified in the database design.

### Dependency Matrix:
- `Ordering` -> depends on `Catalog`, `Inventory`, `Cart`, `Shipping`, `Promotion`, `PaymentRefund`, `EmailSending`.
- `Cart` -> depends on `Catalog`.
- `AfterSale` -> depends on `Ordering`, `Inventory`.
- `PaymentRefund` -> depends on `Ordering`.
- `ReviewNotification` -> depends on `Catalog`, `Ordering`, `IdentifyAccess`.

---

## 4. PostgreSQL Database Schema & Entity Rules

When implementing EF Core Entities or generating SQL migrations, strictly enforce the following schemas, composite keys, and Enums:

### 4.1 Enums Reference
- **IdentifyAccess**: `UserStatus` (`PENDING`, `ACTIVE`, `BLOCKED`, `INACTIVE`), `TokenType` (`EMAIL_VERIFICATION`, `RESET_PASSWORD`).
- **Catalog**: `ProductStatus` (`ACTIVE`, `DISCONTINUED`).
- **Inventory**: `ProductUnitStatus` (`IN_STOCK`, `RESERVED`, `SOLD`, `DAMAGED`).
- **Ordering**: `OrderStatus` (`PENDING`, `APPROVED`, `SHIPPING`, `DELIVERED`, `CANCELLED`), `OrderItemStatus` (`SOLD`, `RETURNED`, `REPLACED`).
- **Promotion**: `VoucherType` (`FIXED`, `PERCENT`).
- **AfterSale**: `ReturnRequestStatus`, `RepairType` (`WARRANTY`, `REPAIR`), `RepairProcessingLocation`, `RepairRequestStatus`.
- **PaymentRefund**: `TransactionMethodType` (`ONLINE`, `CASH`), `TransactionStatus`, `PaymentType`, `RefundType`.
- **ReviewNotification**: `ReviewStatus` (`VISIBLE`, `HIDDEN`, `DELETED`), `ConversationStatus`.

### 4.2 Primary Keys & Composite Keys Rules
Enforce composite primary keys in EF Core configuration (`HasKey(...)`):
- `RolePermission`: Composite PK (`RoleId`, `PermissionId`).
- `UserRole`: Composite PK (`UserId`, `RoleId`).
- `ProductUnit`: Composite PK (`ProductId`, `SerialNumber`).
- `CartItem`: Composite PK (`CartId`, `ProductId`).
- `OrderItem`: Composite PK (`OrderId`, `ProductId`).
- `OrderItemUnit`: Composite PK (`OrderId`, `ProductId`, `SerialNumber`).
- `OrderVoucher`: Composite PK (`OrderId`, `VoucherId`).

---

## 5. Code Generation & Review Instructions

When asked to generate code or build features for TechSales:
1. **Identify the Target Module**: Determine which of the 12 modules the feature belongs to.
2. **Follow Layering**:
   - Define Entities & Domain Enums in `Domain`.
   - Define DTOs, Use Cases, and Service Interfaces in `Application`.
   - Implement EF Core Repository / Configurations in `Infrastructure`.
   - Expose Endpoints in `Presentation`.
3. **Validation & Handling**:
   - Use `Common` module for unified exception handling and API response envelopes (`ApiResponse<T>`).
   - Validate input DTOs before passing to Application handlers.
4. **Data Consistency**:
   - Ensure explicit Serial Number tracking (`SerialNumber`) when transitioning `ProductUnitStatus` from `IN_STOCK` -> `RESERVED` -> `SOLD`.
