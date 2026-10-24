# Customer Features – TechSales Management System

## 1. Authentication & Account Management

### 1.1 Sign Up

Customers can create a new account using email registration.

#### Main Features
- Open Sign Up screen from authentication page
- Enter:
  - Email
  - Password
  - Confirm Password
- Validate input format before submission
- Check duplicated email in database
- Encrypt password before storing
- Create account with `PENDING` status
- Send verification email containing one-time verification link
- Activate account after clicking verification link
- Display successful registration notification

#### Validations
- Required fields cannot be empty
- Email must follow valid email format
- Password must:
  - contain letters
  - contain numbers
  - contain special characters
  - have minimum 8 characters
- Confirm password must match password

#### Security Features
- Password hashing
- Email verification
- One-time activation token

---

### 1.2 Sign In

Customers can log into the system to access member features.

#### Main Features
- Enter email and password
- Validate input format
- Authenticate against database
- Generate JWT session token
- Redirect user to homepage after login

#### Security Features
- Track failed login attempts
- Temporary account lock after 5 failed attempts
- Reset failed attempt counter after successful login

---

### 1.3 Forget Password

Customers can recover forgotten passwords through email verification.

#### Main Features
- Request password reset using registered email
- Send one-time password reset link
- Verify reset token
- Allow customer to set new password
- Encrypt and update new password

#### Security Features
- One-time reset token
- Expiration validation
- Password hashing

---

### 1.4 Update Personal Information

Customers can modify personal profile information.

#### Editable Information
- Full name
- Phone number

#### Main Features
- Load current profile information
- Edit profile form
- Save updated information
- Confirmation dialog before saving
- Real-time UI refresh after update

#### Validations
- Name cannot be empty
- Phone number must contain 10–11 digits
- No alphabetic characters allowed in phone number

---

### 1.5 Change Password

Customers can proactively change account password.

#### Main Features
- Enter:
  - Current password
  - New password
  - Confirm new password
- Verify current password
- Validate new password strength
- Prevent reusing old password
- Save encrypted new password

#### Security Features
- Password hashing
- Current password verification
- Strong password enforcement

---

## 2. Address Management

### 2.1 Set Default Shipping Address

Customers can select a saved address as default shipping address.

#### Main Features
- Open Address Book
- Select existing address
- Mark selected address as default
- Automatically remove previous default address
- Auto-fill checkout form using default address

#### Data Integrity
- Only one default address per user

---

### 2.2 Update Default Address

Customers can edit existing default shipping address.

#### Main Features
- Load current address information
- Edit address form
- Save updated address
- Keep historical order addresses unchanged

#### Editable Fields
- Province/City
- District/Ward
- Detailed house address

---

### 2.3 Add New Shipping Address

Customers can add additional shipping addresses.

#### Main Features
- Add address from:
  - Address Book
  - Checkout page
- Save address to user account
- Reuse saved addresses during future checkout

#### Editable Fields
- Province/City
- District/Ward
- Detailed address

---

## 3. Product Discovery & Browsing

### 3.1 Search Products by Keyword

Customers can search products using keywords.

#### Main Features
- Search bar with autocomplete suggestions
- Real-time keyword suggestions
- Full-text product search
- Paginated search results

#### Search Capabilities
- Product name
- Product keywords
- Related suggestions

---

### 3.2 Filter Products by Category

Customers can narrow product lists using categories.

#### Main Features
- Multi-category filtering
- Add/remove filter dynamically
- Real-time product list update
- Sidebar filter navigation

#### Supported Categories
- Laptop
- Smartphone
- Accessories
- Tablet
- PC Components

---

### 3.3 Sort Products by Price

Customers can reorder product listings based on price.

#### Sort Options
- Price Low → High
- Price High → Low

#### Main Features
- Dynamic sorting without page reload
- Update UI instantly after sorting

---

### 3.4 View Product Details

Customers can inspect detailed product information.

#### Main Features
- Product images
- Product specifications
- Product description
- Product pricing
- Product stock status

#### UI Features
- Responsive product detail page
- High-resolution product gallery
- Technical specification layout

---

### 3.5 Check Inventory Status

System automatically displays real-time stock availability.

#### Main Features
- Real-time stock checking
- Display:
  - In Stock
  - Out of Stock
- Enable/disable purchase buttons based on stock

#### Business Logic
- Hide “Buy Now” button when stock = 0
- Prevent overselling

---

## 4. Shopping Cart Management

### 4.1 Add Product to Cart

Customers can save products into shopping cart.

#### Main Features
- Select quantity before adding
- Validate stock quantity
- Persist cart data even after browser close
- Update cart badge counter
- Success popup notification

#### Smart Behaviors
- Automatically reduce quantity to max available stock if exceeded

---

### 4.2 Update Cart Quantity

Customers can change quantity of items inside cart.

#### Main Features
- Increase/decrease quantity
- Manual quantity input
- Recalculate total price instantly
- Update cart without page refresh

#### Validations
- Quantity must:
  - be positive
  - be numeric
  - be greater than 0

---

### 4.3 Remove Product from Cart

Customers can delete items from shopping cart.

#### Main Features
- Remove selected item
- Update cart total immediately
- Update cart badge counter
- Confirmation notification

---

### 4.4 Select Items for Checkout

Customers can choose specific cart items for checkout.

#### Main Features
- Checkbox selection
- Multi-item checkout
- Dynamic total recalculation
- Preserve unselected items in cart

---

## 5. Checkout & Payment

### 5.1 Apply Voucher

Customers can enter discount voucher codes.

#### Main Features
- Enter voucher code
- Validate voucher conditions
- Apply discount to order total

#### Voucher Conditions
- Minimum order value
- Expiration date
- Usage limit
- User eligibility

---

### 5.2 Choose Payment Method

Customers can choose preferred payment option.

#### Supported Payment Methods
- Bank transfer
- E-wallet
- Online payment gateway

#### Main Features
- Payment method selection UI
- Store selected payment option
- Integrate with online payment services

---

### 5.3 Confirm Order

Customers finalize purchase transaction.

#### Main Features
- Review order summary
- Confirm shipping information
- Confirm payment method
- Atomic transaction processing
- Generate order record

#### Reliability Features
- Prevent partial transaction failure
- Ensure data consistency

---

### 5.4 Online Payment

Customers can complete online payment.

#### Main Features
- Redirect to secure payment gateway
- Verify payment result
- Update order payment status
- Handle successful/failed transactions

#### Security Features
- Secure payment APIs
- Transaction validation

---

### 5.5 Receive Order Confirmation Email

Customers receive order confirmation email after successful purchase.

#### Email Content
- Order ID
- Purchased items
- Total amount
- Shipping information
- Payment information

#### Main Features
- Automated email sending
- Transaction summary delivery

---

## 6. Order Management

### 6.1 View Order History

Customers can see all previous orders.

#### Main Features
- Order listing
- Pagination
- Order summary display

#### Information Displayed
- Order ID
- Order date
- Total price
- Payment status
- Delivery status

---

### 6.2 View Old Order Details

Customers can inspect details of previous orders.

#### Main Features
- Product list
- Product quantity
- Historical price
- Shipping information
- Payment information

#### Reliability Features
- Preserve historical order data even if products change later

---

### 6.3 Track Order Status

Customers can monitor delivery progress.

#### Status Examples
- Pending
- Approved
- Shipping
- Delivered
- Cancelled

#### Main Features
- Timeline-based order tracking
- Real-time status updates

---

### 6.4 Cancel Pending Order

Customers can cancel orders still in `Pending` state.

#### Main Features
- Cancel request button
- Validate order status
- Update order state
- Restore inventory if needed

#### Restrictions
- Only orders with `Pending` status can be cancelled

---

### 6.5 Change Shipping Address

Customers can update shipping address before shipment.

#### Main Features
- Edit shipping address for unshipped orders
- Save updated delivery information

#### Restrictions
- Cannot change address after shipping starts

---

## 7. Product Reviews & Ratings

### 7.1 Review Purchased Products

Customers can submit reviews for purchased products.

#### Main Features
- Star rating
- Text review
- Upload product images
- Submit product feedback

#### Restrictions
- Only verified purchased products can be reviewed

---

### 7.2 Read Reviews from Other Customers

Customers can browse community reviews.

#### Main Features
- Review listing
- Rating display
- User comments
- Review images

#### Benefits
- Help customers evaluate product quality
- Improve purchase decision making

---

## 8. Cross-System Customer Features

### Persistent Shopping Cart
- Cart data remains saved across sessions
- User can continue shopping later

### Responsive User Interface
- Compatible with:
  - Desktop
  - Tablet
  - Mobile devices

### Real-Time UI Updates
- Cart total updates instantly
- Product filtering updates dynamically
- Stock status refreshes automatically

### Security Mechanisms
- Password hashing
- JWT authentication
- Email verification
- Reset password token validation
- Login rate limiting

### Reliability Mechanisms
- Prevent overselling inventory
- Preserve historical order data
- Atomic checkout transaction
- Persistent cart storage

---

## 9. Customer Pages / Screens

### Authentication
- Sign Up Page
- Sign In Page
- Forget Password Page
- Reset Password Page

### Profile Management
- User Profile Page
- Edit Profile Page
- Change Password Page
- Address Book Page

### Shopping
- Homepage
- Product Listing Page
- Product Details Page
- Search Results Page
- Shopping Cart Page
- Checkout Page

### Order Management
- Order History Page
- Order Details Page
- Order Tracking Page

### Review System
- Product Review Section
- Review Submission Form