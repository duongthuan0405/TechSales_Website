using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentRefundModule.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionMethodCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "payment_refund",
                table: "TransactionMethod",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMethod_Code",
                schema: "payment_refund",
                table: "TransactionMethod",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionMethod_Code",
                schema: "payment_refund",
                table: "TransactionMethod");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "payment_refund",
                table: "TransactionMethod");
        }
    }
}
