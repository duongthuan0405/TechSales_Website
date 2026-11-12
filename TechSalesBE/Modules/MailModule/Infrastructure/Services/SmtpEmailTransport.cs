using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MailModule.Application.Interfaces;
using MailModule.Domain.Entities;
using MailModule.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailModule.Infrastructure.Services;

public sealed class SmtpEmailTransport(IOptions<SmtpOptions> options) : IEmailTransport
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        using var email = new MimeMessage();
        email.From.Add(new MailboxAddress(settings.DisplayName, settings.FromEmail));
        foreach (var address in message.To) 
        {
            email.To.Add(MailboxAddress.Parse(address));
        }
        foreach (var address in message.Cc) 
        {
            email.Cc.Add(MailboxAddress.Parse(address));
        }
        foreach (var address in message.Bcc)
        {
            email.Bcc.Add(MailboxAddress.Parse(address));
        }
        email.Subject = message.Subject;

        var body = new BodyBuilder { TextBody = message.TextBody, HtmlBody = message.HtmlBody };
        foreach (var attachment in message.Attachments)
        {
            body.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }
        email.Body = body.ToMessageBody();

        using var client = new SmtpClient { Timeout = settings.TimeoutMilliseconds };
        var security = settings.Security switch
        {
            SmtpSecurity.StartTls => SecureSocketOptions.StartTls,
            SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
            SmtpSecurity.None => SecureSocketOptions.None,
            _ => throw new InvalidOperationException("Unsupported SMTP security mode.")
        };
        await client.ConnectAsync(settings.Host, settings.Port, security, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.UserName))
            await client.AuthenticateAsync(settings.UserName,
                settings.Password ?? throw new InvalidOperationException("SMTP password is required when UserName is set."),
                cancellationToken);
        await client.SendAsync(email, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
