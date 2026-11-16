using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentRefundModule.Migrations
{
    /// <inheritdoc />
    public partial class CREATE_Db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payment_refund");

            migrationBuilder.CreateTable(
                name: "Refund",
                schema: "payment_refund",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReturnRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "PENDING"),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    RefundTransactionRef = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refund", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionMethod",
                schema: "payment_refund",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                schema: "payment_refund",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepairRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    TransactionRef = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_TransactionMethod_TransactionMethodId",
                        column: x => x.TransactionMethodId,
                        principalSchema: "payment_refund",
                        principalTable: "TransactionMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_TransactionMethodId",
                schema: "payment_refund",
                table: "Payment",
                column: "TransactionMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMethod_Name",
                schema: "payment_refund",
                table: "TransactionMethod",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payment",
                schema: "payment_refund");

            migrationBuilder.DropTable(
                name: "Refund",
                schema: "payment_refund");

            migrationBuilder.DropTable(
                name: "TransactionMethod",
                schema: "payment_refund");
        }
    }
}
