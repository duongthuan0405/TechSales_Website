using System.Net.Mail;
using MailModule.Domain.Enums;
using MailModule.Domain.Exceptions;

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

    public required IReadOnlyList<string> To
    {
        get => _to;
        init => _to = ValidateRecipients(value, nameof(To), required: true);
    }

    public IReadOnlyList<string> Cc
    {
        get => _cc;
        init => _cc = ValidateRecipients(value, nameof(Cc));
    }

    public IReadOnlyList<string> Bcc
    {
        get => _bcc;
        init => _bcc = ValidateRecipients(value, nameof(Bcc));
    }

    public required string Subject
    {
        get => _subject;
        init
        {
            if (string.IsNullOrWhiteSpace(value) || value.Contains('\r') || value.Contains('\n'))
                throw new DomainException(DomainExceptionCode.EMAIL_SUBJECT_INVALID, "A non-empty, single-line subject is required.");
            _subject = value;
        }
    }
    public string? TextBody
    {
        get => _textBody;
        init => _textBody = value;
    }

    public string? HtmlBody
    {
        get => _htmlBody;
        init => _htmlBody = value;
    }
    public IReadOnlyList<EmailAttachment> Attachments
    {
        get => _attachments;
        init
        {
            if (value is null)
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENTS_INVALID, "Attachments cannot be null.");
            var snapshot = value.ToArray();
            if (snapshot.Any(attachment => attachment is null))
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENTS_INVALID, "Attachments cannot contain null items.");
            _attachments = Array.AsReadOnly(snapshot);
        }
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TextBody) && string.IsNullOrWhiteSpace(HtmlBody))
            throw new DomainException(DomainExceptionCode.EMAIL_BODY_REQUIRED, "A text or HTML body is required.");
    }

    private static IReadOnlyList<string> ValidateRecipients(IReadOnlyList<string> recipients, string propertyName, bool required = false)
    {
        if (recipients is null)
            throw new DomainException(required ? DomainExceptionCode.EMAIL_RECIPIENT_REQUIRED : DomainExceptionCode.EMAIL_RECIPIENT_INVALID,
                $"{propertyName} recipients cannot be null.");
        var snapshot = recipients.ToArray();
        if (required && snapshot.Length == 0)
            throw new DomainException(DomainExceptionCode.EMAIL_RECIPIENT_REQUIRED, "At least one To recipient is required.");
        foreach (var address in snapshot)
        {
            if (string.IsNullOrWhiteSpace(address) || address.Contains('\r') || address.Contains('\n') ||
                !MailAddress.TryCreate(address, out var parsed) || parsed.Address != address)
                throw new DomainException(DomainExceptionCode.EMAIL_RECIPIENT_INVALID,
                    $"{propertyName} recipients must contain valid email addresses without display names.");
        }
        return Array.AsReadOnly(snapshot);
    }
}
