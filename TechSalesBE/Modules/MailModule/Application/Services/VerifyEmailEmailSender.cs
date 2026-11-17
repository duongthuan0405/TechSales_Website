using System.Net;
using MailModule.Application.Interfaces;
using MailModule.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace MailModule.Application.Services;

public class VerifyEmailEmailSender : IVerifyEmaiEmailSender
{
    private readonly IEmailService _mail;
    private readonly IConfiguration _configuration;

    public VerifyEmailEmailSender(IEmailService mail, IConfiguration configuration)
    {
        _mail = mail;
        _configuration = configuration;
    }

    private readonly string frontendPage = "verify-email";
    private readonly string subject = "Verify your TechSales account";

    public async Task Execute(string toEmail, string token, int expiredInMinutes, CancellationToken cancellationToken = default)
    {
        string text = $"Your verification code is: {token}\nThis code expires in {expiredInMinutes} minutes. Do not share it with anyone.";
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

            string verificationUrl = $"{frontendUrl.TrimEnd('/')}/{frontendPage}?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(token)}";
            string encodedVerificationUrl = WebUtility.HtmlEncode(verificationUrl);
            text += $"\nVerify your email: {verificationUrl}";

            buttonHtml = $"""
                <p style="margin:28px 0;text-align:center;">
                    <a href="{encodedVerificationUrl}" role="button" style="display:inline-block;background-color:#2563eb;color:#ffffff;text-decoration:none;padding:14px 28px;border-radius:8px;font-size:16px;font-weight:bold;">Verify email</a>
                </p>
                <p style="margin:0 0 8px;color:#64748b;font-size:13px;line-height:1.6;">If the button does not work, open this link:</p>
                <p style="margin:0 0 24px;font-size:13px;line-height:1.6;word-break:break-all;">
                    <a href="{encodedVerificationUrl}" style="color:#2563eb;">{encodedVerificationUrl}</a>
                </p>
                """;
        }

        string encodedToken = WebUtility.HtmlEncode(token);
        string encodedSubject = WebUtility.HtmlEncode(subject);
        string html = $"""
            <!DOCTYPE html>
            <html lang="en">
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
                                        <h1 style="margin:0 0 16px;font-size:24px;">Verify your email</h1>
                                        <p style="margin:0 0 24px;font-size:15px;line-height:1.7;">Use the code below to verify your TechSales account.</p>
                                        <div style="padding:20px;background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:8px;text-align:center;">
                                            <span style="font-family:Consolas,monospace;font-size:32px;font-weight:bold;letter-spacing:6px;">{encodedToken}</span>
                                        </div>
                                        <p style="margin:16px 0;color:#475569;font-size:14px;line-height:1.7;">This code expires in <strong>{expiredInMinutes} minutes</strong>. Do not share it with anyone.</p>
                                        {buttonHtml}
                                        <p style="margin:24px 0 0;padding-top:20px;border-top:1px solid #e2e8f0;color:#64748b;font-size:13px;line-height:1.7;">If you did not create this account, you can ignore this email.</p>
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
