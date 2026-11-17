using CommonModule.Domain.Exceptions;
using MailModule.Domain.Exceptions;

namespace MailModule.Domain.Rules;

internal static class EmailAttachmentRules
{
    internal static void EnsureValidFileName(string value)
    {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENT_FILE_NAME_REQUIRED, DomainExceptionMessage.AttachmentFileNameRequired);
            }
    }

    internal static void EnsureContentPresent(byte[] value)
    {
            if (value is null)
            {
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENT_CONTENT_REQUIRED, DomainExceptionMessage.AttachmentContentRequired);
            }
    }

    internal static void EnsureValidContentType(string value)
    {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENT_CONTENT_TYPE_REQUIRED, DomainExceptionMessage.AttachmentTypeRequired);
            }
    }
}
