using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthModule.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateUserSessionsFromOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSession",
                schema: "identify_access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSession_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "identify_access",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_UserId",
                schema: "identify_access",
                table: "UserSession",
                column: "UserId",
                unique: true);

            // Transfer legacy session records before restricting UserToken to OTP usage.
            migrationBuilder.Sql("""
                INSERT INTO identify_access."UserSession"
                    ("Id", "UserId", "SessionHash", "ExpiredAt", "RevokedAt", "CreatedAt")
                SELECT "Id", "UserId", "Value", "ExpiredAt", "UsedAt", "CreatedAt"
                FROM identify_access."UserToken"
                WHERE "Type" = 'ACCESS_SESSION';
                """);

            migrationBuilder.Sql("""
                DELETE FROM identify_access."UserToken"
                WHERE "Type" = 'ACCESS_SESSION';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the previous storage format when rolling back to the old application.
            migrationBuilder.Sql("""
                INSERT INTO identify_access."UserToken"
                    ("Id", "UserId", "Value", "Type", "ExpiredAt", "UsedAt", "CreatedAt")
                SELECT "Id", "UserId", "SessionHash", 'ACCESS_SESSION', "ExpiredAt", "RevokedAt", "CreatedAt"
                FROM identify_access."UserSession"
                WHERE true
                ON CONFLICT ("UserId", "Type") DO UPDATE
                SET "Value" = EXCLUDED."Value",
                    "ExpiredAt" = EXCLUDED."ExpiredAt",
                    "UsedAt" = EXCLUDED."UsedAt",
                    "CreatedAt" = EXCLUDED."CreatedAt";
                """);

            migrationBuilder.DropTable(
                name: "UserSession",
                schema: "identify_access");
        }
    }
}
