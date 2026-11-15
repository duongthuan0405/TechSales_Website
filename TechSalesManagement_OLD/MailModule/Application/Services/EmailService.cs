using MailModule.Application.Interfaces;
using MailModule.Domain.Entities;

namespace MailModule.Application.Services;

public sealed class EmailService(IEmailTransport transport) : IEmailService
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        message.Validate();
        await transport.SendAsync(message, cancellationToken);
    }
}
