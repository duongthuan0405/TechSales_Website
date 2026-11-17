using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthModule.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAuthRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "identify_access",
                table: "Permission",
                columns: new[] { "Id", "Code", "CreatedAt", "Module", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), "auth.customers.read", new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Auth", "Read customers", null },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), "auth.staff.read", new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Auth", "Read staff", null },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), "auth.users.manage", new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Auth", "Create and update accounts", null },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), "auth.customers.lock", new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Auth", "Lock and unlock accounts within hierarchy", null },
                    { new Guid("a0000000-0000-0000-0000-000000000005"), "auth.rbac.manage", new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Auth", "Read and manage roles and permissions", null }
                });

            migrationBuilder.InsertData(
                schema: "identify_access",
                table: "Role",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Technical administration and RBAC", "Technical Admin" },
                    { new Guid("75595ed2-8e03-476c-a59c-864fbc57b1a9"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Customer account", "Customer" },
                    { new Guid("8e2a0a54-e882-4174-ae34-32f299096d13"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sales and customer support staff", "Staff" },
                    { new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6"), new DateTimeOffset(new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Business administration", "Business Admin" }
                });

            migrationBuilder.InsertData(
                schema: "identify_access",
                table: "RolePermission",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") },
                    { new Guid("a0000000-0000-0000-0000-000000000005"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") },
                    { new Guid("a0000000-0000-0000-0000-000000000001"), new Guid("8e2a0a54-e882-4174-ae34-32f299096d13") },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), new Guid("8e2a0a54-e882-4174-ae34-32f299096d13") },
                    { new Guid("a0000000-0000-0000-0000-000000000001"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("75595ed2-8e03-476c-a59c-864fbc57b1a9"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000001"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000002"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000003"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000004"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000005"), new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000001"), new Guid("8e2a0a54-e882-4174-ae34-32f299096d13") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000004"), new Guid("8e2a0a54-e882-4174-ae34-32f299096d13") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000001"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000002"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000003"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a0000000-0000-0000-0000-000000000004"), new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6") });

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("8e2a0a54-e882-4174-ae34-32f299096d13"));

            migrationBuilder.DeleteData(
                schema: "identify_access",
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("c22cf7a1-67f6-479c-a3df-9504f8270fa6"));
        }
    }
}
