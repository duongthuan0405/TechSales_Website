using AuthModule.Domain.Constants;
using AuthModule.Application.Ports;
using MailModule.Application.Interfaces;

namespace TechSalesBE.WebAPI.Integration;

internal sealed class AuthEmailAdapter : IAuthEmailAdapter
{
    private readonly IVerifyEmaiEmailSender _verificationEmailSender;
    private readonly IForgotPasswordEmailSender _forgotPasswordEmailSender;

    public AuthEmailAdapter(
        IVerifyEmaiEmailSender verificationEmailSender,
        IForgotPasswordEmailSender forgotPasswordEmailSender)
    {
        _verificationEmailSender = verificationEmailSender;
        _forgotPasswordEmailSender = forgotPasswordEmailSender;
    }

    public Task SendVerificationAsync(string email, string token, CancellationToken cancellationToken)
    {
        return _verificationEmailSender.Execute(email, token, AuthLimits.OtpLifetimeMinutes, cancellationToken);
    }

    public Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken)
    {
        return _forgotPasswordEmailSender.Execute(email, token, AuthLimits.OtpLifetimeMinutes, cancellationToken);
    }
}
