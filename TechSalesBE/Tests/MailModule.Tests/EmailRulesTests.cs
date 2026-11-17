using CommonModule.Domain.Exceptions;
using MailModule.Domain.Entities;
using Xunit;

namespace MailModule.Tests;

public sealed class EmailRulesTests
{
    [Fact]
    public void Email_preserves_recipient_snapshot_and_validates_body()
    {
        List<string> recipients = new List<string> { "recipient@example.com" };
        EmailMessage message = new EmailMessage
        {
            To = recipients,
            Subject = "Subject",
            HtmlBody = "<p>Body</p>"
        };
        recipients.Clear();
        Assert.Single(message.To);
        message.Validate();
        EmailMessage missingBody = new EmailMessage { To = message.To, Subject = "Subject" };
        Assert.Throws<DomainException>(() => missingBody.Validate());
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("Name <recipient@example.com>")]
    [InlineData("recipient@example.com\r\nBcc: other@example.com")]
    public void Invalid_recipients_are_rejected(string recipient)
    {
        Assert.Throws<DomainException>(() => new EmailMessage
        {
            To = new List<string> { recipient },
            Subject = "Subject"
        });
    }

    [Fact]
    public void Subject_and_attachment_invariants_remain_enforced()
    {
        Assert.Throws<DomainException>(() => new EmailMessage
        {
            To = new List<string> { "recipient@example.com" },
            Subject = "Subject\r\nInjected header"
        });
        Assert.Throws<DomainException>(() => new EmailAttachment("", new byte[] { 1 }));
        Assert.Throws<DomainException>(() => new EmailAttachment("file.txt", null!));
        Assert.Throws<DomainException>(() => new EmailAttachment("file.txt", new byte[] { 1 }, ""));
        byte[] original = new byte[] { 1, 2 };
        EmailAttachment attachment = new EmailAttachment("file.txt", original);
        original[0] = 9;
        byte[] returned = attachment.Content;
        returned[0] = 8;
        Assert.Equal(1, attachment.Content[0]);
    }
}
