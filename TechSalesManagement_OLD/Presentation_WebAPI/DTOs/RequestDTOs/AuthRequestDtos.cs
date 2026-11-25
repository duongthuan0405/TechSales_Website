namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class RegisterRequestDto
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string confirmPassword { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    public string email { get; set; } = string.Empty;
}

public class VerifyEmailRequestDto
{
    public string email { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string email { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;
    public string newPassword { get; set; } = string.Empty;
    public string confirmPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequestDto
{
    public string currentPassword { get; set; } = string.Empty;
    public string newPassword { get; set; } = string.Empty;
    public string confirmPassword { get; set; } = string.Empty;
}
