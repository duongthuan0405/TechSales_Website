using MailModule.Domain.Rules;

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
            EmailAttachmentRules.EnsureValidFileName(value);

            _fileName = value;
        }
    }

    public byte[] Content
    {
        get => (byte[])_content.Clone();
        init
        {
            EmailAttachmentRules.EnsureContentPresent(value);

            _content = (byte[])value.Clone();
        }
    }

    public string ContentType
    {
        get => _contentType;
        init
        {
            EmailAttachmentRules.EnsureValidContentType(value);

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
