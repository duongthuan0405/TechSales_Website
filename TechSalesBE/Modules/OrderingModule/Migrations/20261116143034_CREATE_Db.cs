using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingModule.Migrations
{
    /// <inheritdoc />
    public partial class CREATE_Db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.CreateTable(
                name: "Order",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalProductAmount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    ShippingFee = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    DiscountAmount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    ShippingAddressSnapshot = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                schema: "ordering",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => new { x.OrderId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_OrderItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "ordering",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemUnit",
                schema: "ordering",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemUnit", x => new { x.OrderId, x.ProductId, x.SerialNumber });
                    table.ForeignKey(
                        name: "FK_OrderItemUnit_OrderItem_OrderId_ProductId",
                        columns: x => new { x.OrderId, x.ProductId },
                        principalSchema: "ordering",
                        principalTable: "OrderItem",
                        principalColumns: new[] { "OrderId", "ProductId" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemUnit",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "OrderItem",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "Order",
                schema: "ordering");
        }
    }
}
