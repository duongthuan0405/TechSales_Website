using MailModule.Application.Messages;
using System.Net;
using MailModule.Application.Interfaces;
using MailModule.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace MailModule.Application.Services;

public class ForgotPasswordEmailSender : IForgotPasswordEmailSender
{
    private readonly IEmailService _mail;
    private readonly IConfiguration _configuration;

    public ForgotPasswordEmailSender(IEmailService mail, IConfiguration configuration)
    {
        _mail = mail;
        _configuration = configuration;
    }

    private readonly string frontendPage = "reset-password";
    private readonly string subject = EmailMessageContent.ResetSubject;

    public async Task Execute(string toEmail, string token, int expiredInMinutes, CancellationToken cancellationToken = default)
    {
        string text = EmailMessageContent.ResetText(token, expiredInMinutes);
        string buttonHtml = string.Empty;
        string? frontendUrl = _configuration["AUTH:FrontendUrl"];
        if (!string.IsNullOrWhiteSpace(frontendUrl))
        {
            Uri? uri;
            bool isAbsoluteUrl = Uri.TryCreate(frontendUrl, UriKind.Absolute, out uri);
            if (!isAbsoluteUrl || uri == null)
            {
                throw new InvalidOperationException("AUTH__FrontendUrl must be an absolute HTTP(S) URL.");
            }

            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            {
                throw new InvalidOperationException("AUTH__FrontendUrl must be an absolute HTTP(S) URL.");
            }

            string resetPasswordUrl = $"{frontendUrl.TrimEnd('/')}/{frontendPage}?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(token)}";
            string encodedResetPasswordUrl = WebUtility.HtmlEncode(resetPasswordUrl);
            text += $"\n{EmailMessageContent.ResetButton}: {resetPasswordUrl}";

            buttonHtml = $"""
                <p style="margin:28px 0;text-align:center;">
                    <a href="{encodedResetPasswordUrl}" role="button" style="display:inline-block;background-color:#2563eb;color:#ffffff;text-decoration:none;padding:14px 28px;border-radius:8px;font-size:16px;font-weight:bold;">{EmailMessageContent.ResetButton}</a>
                </p>
                <p style="margin:0 0 8px;color:#64748b;font-size:13px;line-height:1.6;">{EmailMessageContent.LinkFallback}</p>
                <p style="margin:0 0 24px;font-size:13px;line-height:1.6;word-break:break-all;">
                    <a href="{encodedResetPasswordUrl}" style="color:#2563eb;">{encodedResetPasswordUrl}</a>
                </p>
                """;
        }

        string encodedToken = WebUtility.HtmlEncode(token);
        string encodedSubject = WebUtility.HtmlEncode(subject);
        string html = $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{encodedSubject}</title>
            </head>
            <body style="margin:0;padding:24px 12px;background-color:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#0f172a;">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                    <tr>
                        <td align="center">
                            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background-color:#ffffff;border:1px solid #e2e8f0;border-radius:12px;">
                                <tr>
                                    <td style="padding:32px;">
                                        <p style="margin:0 0 24px;color:#2563eb;font-size:22px;font-weight:bold;">TechSales</p>
                                        <h1 style="margin:0 0 16px;font-size:24px;">{EmailMessageContent.ResetTitle}</h1>
                                        <p style="margin:0 0 24px;font-size:15px;line-height:1.7;">{EmailMessageContent.ResetIntro}</p>
                                        <div style="padding:20px;background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:8px;text-align:center;">
                                            <span style="font-family:Consolas,monospace;font-size:32px;font-weight:bold;letter-spacing:6px;">{encodedToken}</span>
                                        </div>
                                        <p style="margin:16px 0;color:#475569;font-size:14px;line-height:1.7;">{EmailMessageContent.ExpirationHtml(expiredInMinutes)}</p>
                                        {buttonHtml}
                                        <p style="margin:24px 0 0;padding-top:20px;border-top:1px solid #e2e8f0;color:#64748b;font-size:13px;line-height:1.7;">{EmailMessageContent.ResetUnrequested}</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

        EmailMessage message = new EmailMessage
        {
            To = new List<string> { toEmail },
            Subject = subject,
            TextBody = text,
            HtmlBody = html
        };
        await _mail.SendAsync(message, cancellationToken);
    }
}
