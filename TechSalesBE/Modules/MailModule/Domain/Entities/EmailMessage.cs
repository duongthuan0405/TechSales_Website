using MailModule.Domain.Rules;

namespace MailModule.Domain.Entities;
public sealed class EmailMessage
{
    private IReadOnlyList<string> _to = Array.Empty<string>();
    private IReadOnlyList<string> _cc = Array.Empty<string>();
    private IReadOnlyList<string> _bcc = Array.Empty<string>();
    private string _subject = string.Empty;
    private string? _textBody;
    private string? _htmlBody;
    private IReadOnlyList<EmailAttachment> _attachments = Array.Empty<EmailAttachment>();
    public required IReadOnlyList<string> To { get => _to; init => _to = EmailRules.ValidateRecipients(value, nameof(To), required: true); }
    public IReadOnlyList<string> Cc { get => _cc; init => _cc = EmailRules.ValidateRecipients(value, nameof(Cc)); }
    public IReadOnlyList<string> Bcc { get => _bcc; init => _bcc = EmailRules.ValidateRecipients(value, nameof(Bcc)); }

    public required string Subject
    {
        get => _subject;
        init
        {
            EmailRules.EnsureValidSubject(value);

            _subject = value;
        }
    }

    public string? TextBody { get => _textBody; init => _textBody = value; }
    public string? HtmlBody { get => _htmlBody; init => _htmlBody = value; }

    public IReadOnlyList<EmailAttachment> Attachments
    {
        get => _attachments;
        init
        {
            EmailRules.EnsureValidAttachments(value);
            EmailAttachment[] snapshot = value.ToArray();
            _attachments = Array.AsReadOnly(snapshot);
        }
    }

    public void Validate()
    {
        EmailRules.EnsureBodyPresent(TextBody, HtmlBody);
    }

}
