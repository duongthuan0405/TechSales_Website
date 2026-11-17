using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShippingModule.Migrations
{
    /// <inheritdoc />
    public partial class FIX_Provice_to_Province_tb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ward_Provice_ProvinceCode",
                schema: "shipping",
                table: "Ward");

            migrationBuilder.DropTable(
                name: "Provice",
                schema: "shipping");

            migrationBuilder.CreateTable(
                name: "Province",
                schema: "shipping",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character(7)", maxLength: 7, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Province", x => x.Code);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Ward_Province_ProvinceCode",
                schema: "shipping",
                table: "Ward",
                column: "ProvinceCode",
                principalSchema: "shipping",
                principalTable: "Province",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ward_Province_ProvinceCode",
                schema: "shipping",
                table: "Ward");

            migrationBuilder.DropTable(
                name: "Province",
                schema: "shipping");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Ward_Provice_ProvinceCode",
                schema: "shipping",
                table: "Ward",
                column: "ProvinceCode",
                principalSchema: "shipping",
                principalTable: "Provice",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
