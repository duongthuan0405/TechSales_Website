using MailModule.Domain.Enums;
using MailModule.Domain.Exceptions;

namespace MailModule.Domain.Entities;

public sealed class EmailAttachment
{
    private string _fileName = string.Empty;
    private byte[] _content = Array.Empty<byte>();
    private string _contentType = "application/octet-stream";

    public string FileName
    {
        get => _fileName;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENT_FILE_NAME_REQUIRED, "An attachment file name is required.");
            _fileName = value;
        }
    }

    public byte[] Content
    {
        get => (byte[])_content.Clone();
        init
        {
            if (value is null)
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENT_CONTENT_REQUIRED, "Attachment content is required.");
            _content = (byte[])value.Clone();
        }
    }

    public string ContentType
    {
        get => _contentType;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException(DomainExceptionCode.EMAIL_ATTACHMENT_CONTENT_TYPE_REQUIRED, "An attachment MIME type is required.");
            _contentType = value;
        }
    }

    public EmailAttachment(string FileName, byte[] Content, string ContentType = "application/octet-stream")
    {
        this.FileName = FileName;
        this.Content = Content;
        this.ContentType = ContentType;
    }
}
