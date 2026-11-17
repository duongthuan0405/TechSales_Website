namespace MailModule.Application.Interfaces;

public interface IForgotPasswordEmailSender
{
    Task Execute(string toEmail, string token, int expiredInMinutes, CancellationToken cancellationToken = default);
}
