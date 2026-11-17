using MailModule.Application.Interfaces;
using MailModule.Domain.Entities;

namespace MailModule.Application.Services;
public sealed class EmailService : IEmailService
{
    private readonly IEmailTransport _transport;

    public EmailService(IEmailTransport transport)
    {
        _transport = transport;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        message.Validate();
        await _transport.SendAsync(message, cancellationToken);
    }
}
