using MailModule.Domain.Entities;

namespace MailModule.Application.Interfaces;

/// <summary>Delivery port. Implement this interface to replace SMTP with another provider.</summary>
public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
