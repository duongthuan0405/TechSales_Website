using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSalesManagement.Migrations
{
    /// <inheritdoc />
    public partial class ADD_ApprovedAt_ShippedAt_DeliveredAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "approved_at",
                table: "Order",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "delivered_at",
                table: "Order",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "shipped_at",
                table: "Order",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_at",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "delivered_at",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "shipped_at",
                table: "Order");
        }
    }
}
