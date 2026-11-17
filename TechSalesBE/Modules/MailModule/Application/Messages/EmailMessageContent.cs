namespace MailModule.Application.Messages;

public static class EmailMessageContent
{
    public const string VerificationSubject = "Xác nhận tài khoản TechSales";
    public const string ResetSubject = "Đặt lại mật khẩu TechSales";
    public const string VerificationTitle = "Xác nhận email của bạn";
    public const string ResetTitle = "Đặt lại mật khẩu của bạn";
    public const string VerificationButton = "Xác nhận email";
    public const string ResetButton = "Đặt lại mật khẩu";
    public const string VerificationIntro = "Vui lòng dùng mã bên dưới để xác nhận tài khoản TechSales của bạn.";
    public const string ResetIntro = "Chúng tôi nhận được yêu cầu đặt lại mật khẩu TechSales. Vui lòng dùng mã bên dưới hoặc nút đặt lại mật khẩu để chọn mật khẩu mới.";
    public const string LinkFallback = "Nếu nút bấm không hoạt động, vui lòng mở liên kết sau:";
    public const string VerificationUnrequested = "Nếu bạn không tạo tài khoản này, vui lòng bỏ qua email.";
    public const string ResetUnrequested = "Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email. Mật khẩu của bạn sẽ không thay đổi.";

    public static string VerificationText(string token, int expiredInMinutes)
    {
        return $"Mã xác nhận của bạn là: {token}\nMã có hiệu lực trong {expiredInMinutes} phút. Không chia sẻ mã với bất kỳ ai.";
    }

    public static string ResetText(string token, int expiredInMinutes)
    {
        return $"Mã đặt lại mật khẩu của bạn là: {token}\nMã có hiệu lực trong {expiredInMinutes} phút. Không chia sẻ mã với bất kỳ ai.";
    }

    public static string ExpirationHtml(int expiredInMinutes)
    {
        return $"Mã có hiệu lực trong <strong>{expiredInMinutes} phút</strong>. Không chia sẻ mã với bất kỳ ai.";
    }
}
