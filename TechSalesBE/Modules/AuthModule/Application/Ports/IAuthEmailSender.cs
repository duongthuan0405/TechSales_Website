namespace AuthModule.Application.Ports;
// Consumer-owned port; the host connects it to MailModule.
public interface IAuthEmailAdapter
{
    Task SendVerificationAsync(string email, string token, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken);
}
