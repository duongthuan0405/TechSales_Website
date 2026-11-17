using CommonModule.Domain.Exceptions;
using MailModule.Domain.Entities;
using MailModule.Domain.Exceptions;
using System.Net.Mail;

namespace MailModule.Domain.Rules;

internal static class EmailRules
{
    internal static void EnsureValidSubject(string subject)
    {
            if (string.IsNullOrWhiteSpace(subject) || subject.Contains('\r') || subject.Contains('\n'))
            {
                throw new DomainException(DomainExceptionCode.EMAIL_SUBJECT_INVALID, DomainExceptionMessage.EmailSubjectInvalid);
            }
    }

    internal static void EnsureValidAttachments(IReadOnlyList<EmailAttachment> attachments)
    {
            if (attachments is null)
            {
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENTS_INVALID, DomainExceptionMessage.AttachmentsRequired);
            }

            EmailAttachment[] snapshot = attachments.ToArray();
            if (snapshot.Any(attachment => attachment is null))
            {
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENTS_INVALID, DomainExceptionMessage.AttachmentsContainNull);
            }

    }

    internal static void EnsureBodyPresent(string? textBody, string? htmlBody)
    {
        if (string.IsNullOrWhiteSpace(textBody) && string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new DomainException(DomainExceptionCode.EMAIL_BODY_REQUIRED, DomainExceptionMessage.EmailBodyRequired);
        }
    }

    internal static IReadOnlyList<string> ValidateRecipients(IReadOnlyList<string> recipients, string propertyName, bool required = false)
    {
        if (recipients is null)
        {
            throw new DomainException(required ? DomainExceptionCode.EMAIL_RECIPIENT_REQUIRED : DomainExceptionCode.EMAIL_RECIPIENT_INVALID, DomainExceptionMessage.RecipientsRequired(propertyName));
        }

        string[] snapshot = recipients.ToArray();
        if (required && snapshot.Length == 0)
        {
            throw new DomainException(DomainExceptionCode.EMAIL_RECIPIENT_REQUIRED, DomainExceptionMessage.EmailRecipientRequired);
        }

        foreach (string address in snapshot)
        {
            if (string.IsNullOrWhiteSpace(address) || address.Contains('\r') || address.Contains('\n') || !MailAddress.TryCreate(address, out var parsed) || parsed.Address != address)
            {
                throw new DomainException(DomainExceptionCode.EMAIL_RECIPIENT_INVALID, DomainExceptionMessage.RecipientsInvalid(propertyName));
            }
        }

        return Array.AsReadOnly(snapshot);
    }
}
