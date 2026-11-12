using MailModule.Domain.Entities;

namespace MailModule.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
