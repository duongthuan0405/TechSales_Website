using System;
using System.Threading.Tasks;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterParams parameters);
    Task<User> LoginAsync(LoginParams parameters);
    Task VerifyEmailAsync(VerifyEmailParams parameters);
    Task ForgotPasswordAsync(ForgotPasswordParams parameters);
    Task ResetPasswordAsync(ResetPasswordParams parameters);
    Task ChangePasswordAsync(ChangePasswordParams parameters);
    Task LogoutAsync(string token);
}
