using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthModule.Infrastructure.Persistence.Migrations;

public partial class RenameUserTokenToUserOtp : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_UserToken_User_UserId", "UserToken", "identify_access");
        migrationBuilder.DropPrimaryKey("PK_UserToken", "UserToken", "identify_access");
        migrationBuilder.RenameTable(name: "UserToken", schema: "identify_access", newName: "UserOtp", newSchema: "identify_access");
        migrationBuilder.RenameIndex(name: "IX_UserToken_UserId_Type", table: "UserOtp", newName: "IX_UserOtp_UserId_Type", schema: "identify_access");
        migrationBuilder.AddPrimaryKey("PK_UserOtp", "UserOtp", "Id", "identify_access");
        migrationBuilder.AddForeignKey(name: "FK_UserOtp_User_UserId", table: "UserOtp", column: "UserId", principalTable: "User", schema: "identify_access", principalSchema: "identify_access", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_UserOtp_User_UserId", "UserOtp", "identify_access");
        migrationBuilder.DropPrimaryKey("PK_UserOtp", "UserOtp", "identify_access");
        migrationBuilder.RenameTable(name: "UserOtp", schema: "identify_access", newName: "UserToken", newSchema: "identify_access");
        migrationBuilder.RenameIndex(name: "IX_UserOtp_UserId_Type", table: "UserToken", newName: "IX_UserToken_UserId_Type", schema: "identify_access");
        migrationBuilder.AddPrimaryKey("PK_UserToken", "UserToken", "Id", "identify_access");
        migrationBuilder.AddForeignKey(name: "FK_UserToken_User_UserId", table: "UserToken", column: "UserId", principalTable: "User", schema: "identify_access", principalSchema: "identify_access", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }
}