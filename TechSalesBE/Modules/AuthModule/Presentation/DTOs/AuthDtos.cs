using Microsoft.AspNetCore.Http;
namespace AuthModule.Presentation.DTOs;
public sealed record RegisterRequestDto(string Email, string Password, string ConfirmPassword, string FullName, string Phone);
public sealed record LoginRequestDto(string Email, string Password);
public sealed record LoginResponseDto(string Token, string Email, List<string> Roles, DateTimeOffset ExpiresAt);
public sealed record RefreshTokenResponseDto(string Token, DateTimeOffset ExpiresAt);
public sealed record VerifyEmailRequestDto(string Email, string Token);
public sealed record ForgotPasswordRequestDto(string Email);
public sealed record ResetPasswordRequestDto(string Email, string Token, string NewPassword, string ConfirmPassword);
public sealed record ChangePasswordRequestDto(string CurrentPassword, string NewPassword, string ConfirmPassword);
public sealed record CreateUserRequestDto(string Email, string Password, string[] Roles, string FullName, string Phone);
public sealed record UpdateUserRequestDto(string Email);
public sealed record LockUserRequestDto(DateTimeOffset? Until);
public sealed record UpdatePermissionsRequestDto(Guid[] PermissionIds);
public sealed record AssignRolesRequestDto(Guid[] RoleIds);
public sealed class UpdateProfileRequestDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public IFormFile? AvatarFile { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
