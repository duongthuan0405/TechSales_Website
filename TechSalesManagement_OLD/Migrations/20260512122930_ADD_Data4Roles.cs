using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechSalesManagement.Migrations
{
    /// <inheritdoc />
    public partial class ADD_Data4Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "id", "created_at", "description", "name" },
                values: new object[,]
                {
                    { new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Technical Administrator with full system access", "Technical Admin" },
                    { new Guid("75595ed2-8e03-476c-a59c-864fbc57b1a9"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Default customer access", "Customer" },
                    { new Guid("8e2a0a54-e882-4174-ae34-32f299096d13"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sales Staff member access", "Staff" },
                    { new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Business Administrator for management tasks", "Business Admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: new Guid("75595ed2-8e03-476c-a59c-864fbc57b1a9"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: new Guid("8e2a0a54-e882-4174-ae34-32f299096d13"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6"));
        }
    }
}
