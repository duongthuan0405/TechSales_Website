using System;

namespace TechSalesManagement.Application.Services.Params;

public class RegisterParams
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class LoginParams
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class VerifyEmailParams
{
    public required string Email { get; set; }
    public required string Token { get; set; }
}

public class ForgotPasswordParams
{
    public required string Email { get; set; }
}

public class ResetPasswordParams
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class ChangePasswordParams
{
    public required Guid UserId { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}
