using AuthModule.Domain.Constants;

namespace AuthModule.Domain.Exceptions;

public static class DomainExceptionMessage
{
    public const string UserFilterForbidden = "Bạn không được phép lọc loại người dùng này.";
    public const string AccountInactive = "Tài khoản chưa được kích hoạt hoặc đã bị vô hiệu hóa.";
    public const string AccountNotLocked = "Tài khoản hiện không bị khóa.";
    public const string AccountNotPending = "Tài khoản không ở trạng thái chờ xác nhận email.";
    public const string ActiveActorRequired = "Chỉ tài khoản đang hoạt động được quản lý tài khoản khác.";
    public const string AvatarInvalid = "Đường dẫn ảnh đại diện không hợp lệ.";
    public const string BirthDateInvalid = "Ngày sinh không hợp lệ.";
    public const string CannotCreateAdmin = "Quản trị viên kinh doanh chỉ được tạo tài khoản khách hàng hoặc nhân viên.";
    public const string CannotCreateTechnicalAdmin = CannotCreateAdmin;
    public const string CannotManageAccount = "Bạn không được phép quản lý tài khoản này.";
    public const string CannotManageAdmin = "Bạn không được phép quản lý tài khoản quản trị này.";
    public const string CannotManageSelf = "Vui lòng dùng chức năng hồ sơ cá nhân hoặc đổi mật khẩu để quản lý tài khoản của bạn.";
    public const string CurrentPasswordIncorrect = "Mật khẩu hiện tại không chính xác.";
    public const string EmailAlreadyRegistered = "Địa chỉ email đã được đăng ký.";
    public const string EmailInvalid = "Địa chỉ email không hợp lệ.";
    public const string FullNameInvalid = "Họ và tên không hợp lệ.";
    public const string LockExpiryInPast = "Thời điểm hết hạn khóa phải ở tương lai.";
    public const string OtpInvalid = "Mã xác nhận không hợp lệ hoặc đã hết hạn.";
    public const string PasswordHashInvalid = "Dữ liệu mật khẩu không hợp lệ.";
    public const string PasswordInvalid = "Mật khẩu không hợp lệ.";
    public static string PasswordRequirement => $"Mật khẩu phải có ít nhất {AuthLimits.PasswordMinLength} ký tự và tối đa {AuthLimits.PasswordMaxUtf8Bytes} byte UTF-8.";
    public const string PasswordUnchanged = "Mật khẩu mới phải khác mật khẩu hiện tại.";
    public const string PermissionUnknown = "Quyền không tồn tại.";
    public const string PhoneInvalid = "Số điện thoại không hợp lệ.";
    public static string PhoneRequirement => $"Số điện thoại phải có từ {AuthLimits.PhoneMinDigits} đến {AuthLimits.PhoneMaxDigits} chữ số.";
    public const string RoleUnknown = "Vai trò không tồn tại.";
    public const string StaffCanOnlyManageCustomers = "Nhân viên chỉ được quản lý tài khoản khách hàng.";
    public const string TechnicalAdminPermissionsProtected = "Không được thay đổi quyền của quản trị viên kỹ thuật.";
    public const string TechnicalAdminRequired = "Chỉ quản trị viên kỹ thuật được phân công vai trò.";
    public const string UsernameInvalid = "Tên đăng nhập không hợp lệ.";
    public const string VerifiedAccountRequired = "Chỉ có thể khóa tài khoản đã xác nhận email.";
}
