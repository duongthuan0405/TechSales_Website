using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSalesManagement.Migrations
{
    /// <inheritdoc />
    public partial class ADD_ForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserRole_role_id",
                table: "UserRole",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_permission_id",
                table: "RolePermission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewResponse_review_id",
                table: "ReviewResponse",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewResponse_user_id",
                table: "ReviewResponse",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_user_id",
                table: "Review",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_category_id",
                table: "Product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_payment_method_id",
                table: "Payment",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderVoucher_voucher_id",
                table: "OrderVoucher",
                column: "voucher_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_product_id",
                table: "OrderItem",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_user_id",
                table: "Order",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_product_id",
                table: "CartItem",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_user_id",
                table: "AuditLog",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_User_user_id",
                table: "AuditLog",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_User_user_id",
                table: "Cart",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_Cart_cart_id",
                table: "CartItem",
                column: "cart_id",
                principalTable: "Cart",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_Product_product_id",
                table: "CartItem",
                column: "product_id",
                principalTable: "Product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Product_product_id",
                table: "Inventory",
                column: "product_id",
                principalTable: "Product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_User_user_id",
                table: "Notification",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_User_user_id",
                table: "Order",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Order_order_id",
                table: "OrderItem",
                column: "order_id",
                principalTable: "Order",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Product_product_id",
                table: "OrderItem",
                column: "product_id",
                principalTable: "Product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderVoucher_Order_order_id",
                table: "OrderVoucher",
                column: "order_id",
                principalTable: "Order",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderVoucher_Voucher_voucher_id",
                table: "OrderVoucher",
                column: "voucher_id",
                principalTable: "Voucher",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Order_order_id",
                table: "Payment",
                column: "order_id",
                principalTable: "Order",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_PaymentMethod_payment_method_id",
                table: "Payment",
                column: "payment_method_id",
                principalTable: "PaymentMethod",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Category_category_id",
                table: "Product",
                column: "category_id",
                principalTable: "Category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Product_product_id",
                table: "ProductImage",
                column: "product_id",
                principalTable: "Product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Product_product_id",
                table: "Review",
                column: "product_id",
                principalTable: "Product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_User_user_id",
                table: "Review",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewResponse_Review_review_id",
                table: "ReviewResponse",
                column: "review_id",
                principalTable: "Review",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewResponse_User_user_id",
                table: "ReviewResponse",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Permission_permission_id",
                table: "RolePermission",
                column: "permission_id",
                principalTable: "Permission",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Role_role_id",
                table: "RolePermission",
                column: "role_id",
                principalTable: "Role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingAddress_User_user_id",
                table: "ShippingAddress",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_User_user_id",
                table: "UserProfile",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Role_role_id",
                table: "UserRole",
                column: "role_id",
                principalTable: "Role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_User_user_id",
                table: "UserRole",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserToken_User_user_id",
                table: "UserToken",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_User_user_id",
                table: "AuditLog");

            migrationBuilder.DropForeignKey(
                name: "FK_Cart_User_user_id",
                table: "Cart");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_Cart_cart_id",
                table: "CartItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_Product_product_id",
                table: "CartItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Product_product_id",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_User_user_id",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_User_user_id",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Order_order_id",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Product_product_id",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderVoucher_Order_order_id",
                table: "OrderVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderVoucher_Voucher_voucher_id",
                table: "OrderVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Order_order_id",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_PaymentMethod_payment_method_id",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Category_category_id",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Product_product_id",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Product_product_id",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_User_user_id",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewResponse_Review_review_id",
                table: "ReviewResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewResponse_User_user_id",
                table: "ReviewResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Permission_permission_id",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_role_id",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingAddress_User_user_id",
                table: "ShippingAddress");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_User_user_id",
                table: "UserProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Role_role_id",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_User_user_id",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserToken_User_user_id",
                table: "UserToken");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_role_id",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_permission_id",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_ReviewResponse_review_id",
                table: "ReviewResponse");

            migrationBuilder.DropIndex(
                name: "IX_ReviewResponse_user_id",
                table: "ReviewResponse");

            migrationBuilder.DropIndex(
                name: "IX_Review_user_id",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Product_category_id",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Payment_payment_method_id",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_OrderVoucher_voucher_id",
                table: "OrderVoucher");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_product_id",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_Order_user_id",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_CartItem_product_id",
                table: "CartItem");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_user_id",
                table: "AuditLog");
        }
    }
}
