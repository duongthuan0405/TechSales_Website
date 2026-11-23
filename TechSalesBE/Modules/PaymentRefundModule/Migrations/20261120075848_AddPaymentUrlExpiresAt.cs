using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentRefundModule.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentUrlExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentUrlExpiresAt",
                schema: "payment_refund",
                table: "Payment",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentUrlExpiresAt",
                schema: "payment_refund",
                table: "Payment");
        }
    }
}
