using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfterSaleModule.Migrations
{
    /// <inheritdoc />
    public partial class CREATE_Db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "after_sale");

            migrationBuilder.CreateTable(
                name: "RepairRequest",
                schema: "after_sale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "PENDING"),
                    ProblemDescription = table.Column<string>(type: "text", nullable: true),
                    ReceivedDeviceAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequestReason",
                schema: "after_sale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequestReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnResultProductUnit",
                schema: "after_sale",
                columns: table => new
                {
                    ReturnResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ResultCheckingDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnResultProductUnit", x => new { x.ReturnResultId, x.ProductId, x.SerialNumber });
                });

            migrationBuilder.CreateTable(
                name: "RepairResult",
                schema: "after_sale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepairType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "REPAIR"),
                    ProcessingLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CheckingResult = table.Column<string>(type: "text", nullable: true),
                    SolutionDescription = table.Column<string>(type: "text", nullable: true),
                    TransferredToManufactureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnedToStoreAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairResult_RepairRequest_Id",
                        column: x => x.Id,
                        principalSchema: "after_sale",
                        principalTable: "RepairRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequest",
                schema: "after_sale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ReasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonDescription = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequest_ReturnRequestReason_ReasonId",
                        column: x => x.ReasonId,
                        principalSchema: "after_sale",
                        principalTable: "ReturnRequestReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequestImage",
                schema: "after_sale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReturnRequestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequestImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequestImage_ReturnRequest_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalSchema: "after_sale",
                        principalTable: "ReturnRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnResult",
                schema: "after_sale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ResultCheckingDescription = table.Column<string>(type: "text", nullable: true),
                    TotalRefundAmount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnResult_ReturnRequest_Id",
                        column: x => x.Id,
                        principalSchema: "after_sale",
                        principalTable: "ReturnRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequest_ReasonId",
                schema: "after_sale",
                table: "ReturnRequest",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestImage_ReturnRequestId",
                schema: "after_sale",
                table: "ReturnRequestImage",
                column: "ReturnRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairResult",
                schema: "after_sale");

            migrationBuilder.DropTable(
                name: "ReturnRequestImage",
                schema: "after_sale");

            migrationBuilder.DropTable(
                name: "ReturnResult",
                schema: "after_sale");

            migrationBuilder.DropTable(
                name: "ReturnResultProductUnit",
                schema: "after_sale");

            migrationBuilder.DropTable(
                name: "RepairRequest",
                schema: "after_sale");

            migrationBuilder.DropTable(
                name: "ReturnRequest",
                schema: "after_sale");

            migrationBuilder.DropTable(
                name: "ReturnRequestReason",
                schema: "after_sale");
        }
    }
}
