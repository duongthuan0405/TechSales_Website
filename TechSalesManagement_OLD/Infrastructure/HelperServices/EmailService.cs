using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using TechSalesManagement.Application.Common.Configurations;
using TechSalesManagement.Application.HelperServices;

namespace TechSalesManagement.Infrastructure.HelperServices;

public class EmailService : IEmailService
{
    private readonly MailSettingsCO _mailSettings;

    public EmailService(IOptions<MailSettingsCO> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_mailSettings.displayName, _mailSettings.fromEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            // Connect securely using MailKit logic
            await smtp.ConnectAsync(_mailSettings.host, _mailSettings.port, MailKit.Security.SecureSocketOptions.StartTls);
            
            if (!string.IsNullOrEmpty(_mailSettings.password))
            {
                await smtp.AuthenticateAsync(_mailSettings.userName, _mailSettings.password);
            }
            
            await smtp.SendAsync(email);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }

    public async Task SendVerificationEmailAsync(string to, string verificationLink)
    {
        string subject = "TechSales - Account Verification";
        string body = $@"
            <h3>Welcome to TechSales,</h3>
            <p>Thank you for registering. Please use this link (or enter the provided OTP) to verify your account:</p>
            <p><a href='{verificationLink}' style='background-color:#007bff;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Click here to verify</a></p>
            <p>Alternatively, you can copy and paste this URL into your browser:</p>
            <p>{verificationLink}</p>
            <br/>
            <p>Best regards,</p>
            <p>The TechSales Team</p>
        ";
        
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetLink)
    {
        string subject = "TechSales - Password Reset Request";
        string body = $@"
            <h3>Hello,</h3>
            <p>We received a request to reset your TechSales account password.</p>
            <p><a href='{resetLink}' style='background-color:#28a745;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Reset My Password</a></p>
            <p>If you did not request a password reset, please ignore this email.</p>
            <br/>
            <p>Best regards,</p>
            <p>The TechSales Team</p>
        ";
        
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendOrderConfirmationEmailAsync(string to, Guid orderId, decimal totalAmount, string shippingAddress)
    {
        string subject = $"TechSales - Order Confirmed #{orderId.ToString().Substring(0, 8).ToUpper()}";
        string body = $@"
            <div style='font-family:Segoe UI, Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 30px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                <h2 style='color: #2c3e50; text-align: center; margin-bottom: 30px;'>🎉 Order Confirmed! 🎉</h2>
                <p>Hi there,</p>
                <p>Thank you for shopping with <strong>TechSales</strong>! We are thrilled to let you know that your order has been successfully placed and is now being processed.</p>
                
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 25px 0;'>
                    <h4 style='margin-top:0; color: #2c3e50; border-bottom: 1px solid #dee2e6; padding-bottom: 10px;'>Order Details</h4>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 8px 0; color: #6c757d;'>Order ID:</td>
                            <td style='padding: 8px 0; font-weight: bold; color: #212529;'>#{orderId.ToString().ToUpper()}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px 0; color: #6c757d;'>Total Amount:</td>
                            <td style='padding: 8px 0; font-weight: bold; color: #28a745; font-size: 1.1em;'>{totalAmount:N0} VND</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px 0; color: #6c757d;'>Shipping To:</td>
                            <td style='padding: 8px 0; color: #212529;'>{shippingAddress}</td>
                        </tr>
                    </table>
                </div>

                <p style='text-align: center; margin: 35px 0;'>
                    <a href='https://techsales.com/account/orders' style='background: linear-gradient(135deg, #4e54c8 0%, #8f94fb 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; box-shadow: 0 4px 10px rgba(78,84,200,0.3);'>View Order Status</a>
                </p>

                <hr style='border: 0; border-top: 1px solid #eee; margin: 30px 0;' />
                <p style='font-size: 0.85em; color: #999; text-align: center;'>If you have any questions, please contact our support team.<br/>&copy; {DateTime.UtcNow.Year} TechSales Management. All rights reserved.</p>
            </div>
        ";

        await SendEmailAsync(to, subject, body);
    }
}
