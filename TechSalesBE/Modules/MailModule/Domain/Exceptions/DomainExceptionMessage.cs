namespace MailModule.Domain.Exceptions;

public static class DomainExceptionMessage
{
    public const string AttachmentContentRequired = "Vui lòng cung cấp nội dung tệp đính kèm.";
    public const string AttachmentFileNameRequired = "Vui lòng cung cấp tên tệp đính kèm.";
    public const string AttachmentTypeRequired = "Vui lòng cung cấp định dạng tệp đính kèm.";
    public const string AttachmentsContainNull = "Danh sách tệp đính kèm không được chứa phần tử null.";
    public const string AttachmentsRequired = "Danh sách tệp đính kèm không được null.";
    public const string EmailBodyRequired = "Vui lòng cung cấp nội dung email dạng văn bản hoặc HTML.";
    public const string EmailRecipientRequired = "Email phải có ít nhất một người nhận.";
    public const string EmailSubjectInvalid = "Tiêu đề email không được để trống hoặc chứa ký tự xuống dòng.";
    public static string RecipientsInvalid(string propertyName)
    {
        return $"Danh sách người nhận {propertyName} phải chứa địa chỉ email hợp lệ, không kèm tên hiển thị.";
    }
    public static string RecipientsRequired(string propertyName)
    {
        return $"Danh sách người nhận {propertyName} không được null.";
    }
}
