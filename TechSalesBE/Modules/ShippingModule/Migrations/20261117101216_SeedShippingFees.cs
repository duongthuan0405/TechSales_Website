using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShippingModule.Migrations
{
    /// <inheritdoc />
    public partial class SeedShippingFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "shipping",
                table: "ShippingFee",
                columns: new[] { "ProviceCode", "Fee" },
                values: new object[,]
                {
                    { "11", 10000L },
                    { "12", 10000L },
                    { "13", 10000L },
                    { "14", 10000L },
                    { "15", 10000L },
                    { "16", 10000L },
                    { "17", 10000L },
                    { "18", 10000L },
                    { "19", 10000L },
                    { "20", 10000L },
                    { "21", 10000L },
                    { "22", 10000L },
                    { "23", 10000L },
                    { "24", 10000L },
                    { "25", 10000L },
                    { "26", 10000L },
                    { "27", 10000L },
                    { "28", 10000L },
                    { "29", 10000L },
                    { "30", 10000L },
                    { "31", 10000L },
                    { "32", 10000L },
                    { "33", 10000L },
                    { "34", 10000L },
                    { "35", 10000L },
                    { "36", 10000L },
                    { "37", 10000L },
                    { "38", 10000L },
                    { "39", 10000L },
                    { "40", 10000L },
                    { "41", 10000L },
                    { "42", 10000L },
                    { "43", 10000L },
                    { "44", 10000L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "11");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "12");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "13");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "14");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "15");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "16");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "17");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "18");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "19");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "20");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "21");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "22");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "23");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "24");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "25");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "26");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "27");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "28");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "29");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "30");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "31");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "32");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "33");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "34");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "35");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "36");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "37");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "38");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "39");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "40");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "41");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "42");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "43");

            migrationBuilder.DeleteData(
                schema: "shipping",
                table: "ShippingFee",
                keyColumn: "ProviceCode",
                keyValue: "44");
        }
    }
}
