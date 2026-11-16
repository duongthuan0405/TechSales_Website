using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShippingModule.Migrations
{
    /// <inheritdoc />
    public partial class CREATE_Db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shipping");

            migrationBuilder.CreateTable(
                name: "Provice",
                schema: "shipping",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character(7)", maxLength: 7, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provice", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ShippingFee",
                schema: "shipping",
                columns: table => new
                {
                    ProviceCode = table.Column<string>(type: "character(7)", maxLength: 7, nullable: false),
                    Fee = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingFee", x => x.ProviceCode);
                });

            migrationBuilder.CreateTable(
                name: "Ward",
                schema: "shipping",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character(7)", maxLength: 7, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProvinceCode = table.Column<string>(type: "character(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ward", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Ward_Provice_ProvinceCode",
                        column: x => x.ProvinceCode,
                        principalSchema: "shipping",
                        principalTable: "Provice",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShippingAddress",
                schema: "shipping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WardCode = table.Column<string>(type: "character(7)", maxLength: 7, nullable: false),
                    Detail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdateAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingAddress_Ward_WardCode",
                        column: x => x.WardCode,
                        principalSchema: "shipping",
                        principalTable: "Ward",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingAddress_WardCode",
                schema: "shipping",
                table: "ShippingAddress",
                column: "WardCode");

            migrationBuilder.CreateIndex(
                name: "IX_Ward_ProvinceCode",
                schema: "shipping",
                table: "Ward",
                column: "ProvinceCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingAddress",
                schema: "shipping");

            migrationBuilder.DropTable(
                name: "ShippingFee",
                schema: "shipping");

            migrationBuilder.DropTable(
                name: "Ward",
                schema: "shipping");

            migrationBuilder.DropTable(
                name: "Provice",
                schema: "shipping");
        }
    }
}
