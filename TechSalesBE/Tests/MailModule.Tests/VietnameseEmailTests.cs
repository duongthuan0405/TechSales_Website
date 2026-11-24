using MailModule.Application.Interfaces;
using MailModule.Application.Services;
using MailModule.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MailModule.Tests;

public sealed class VietnameseEmailTests
{
    private sealed class CapturedEmailService : IEmailService
    {
        public List<EmailMessage> Messages { get; } = new List<EmailMessage>();

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Verification_and_reset_emails_use_vietnamese(bool hasFrontend)
    {
        Dictionary<string, string?> values = new Dictionary<string, string?>();
        if (hasFrontend)
        {
            values.Add("AUTH:FrontendUrl", "https://frontend.example.com");
        }

        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        CapturedEmailService captured = new CapturedEmailService();
        VerifyEmailEmailSender verification = new VerifyEmailEmailSender(captured, configuration);
        ForgotPasswordEmailSender reset = new ForgotPasswordEmailSender(captured, configuration);
        await verification.Execute("user@example.com", "123456", 7);
        await reset.Execute("user@example.com", "654321", 7);

        Assert.Equal("Xác nhận tài khoản TechSales", captured.Messages[0].Subject);
        Assert.Equal("Đặt lại mật khẩu TechSales", captured.Messages[1].Subject);
        foreach (EmailMessage message in captured.Messages)
        {
            Assert.Contains("7 phút", message.TextBody);
            Assert.Contains("lang=\"vi\"", message.HtmlBody);
            Assert.DoesNotContain("This code expires", message.HtmlBody);
            Assert.DoesNotContain("If you did not", message.HtmlBody);
        }

        if (hasFrontend)
        {
            Assert.Contains("https://frontend.example.com/auth/verify-email?email=user%40example.com&token=123456", captured.Messages[0].TextBody);
            Assert.Contains("https://frontend.example.com/auth/reset-password?email=user%40example.com&token=654321", captured.Messages[1].TextBody);
            Assert.Contains(">Xác nhận email</a>", captured.Messages[0].HtmlBody);
            Assert.Contains(">Đặt lại mật khẩu</a>", captured.Messages[1].HtmlBody);
        }
    }
}
