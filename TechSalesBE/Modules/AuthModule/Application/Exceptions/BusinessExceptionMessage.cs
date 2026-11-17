using AuthModule.Domain.Constants;

namespace AuthModule.Application.Exceptions;

public static class BusinessExceptionMessage
{
    public const string AccessDenied = "Bạn không có quyền thực hiện thao tác này.";
    public const string AccountInactive = "Tài khoản chưa được kích hoạt hoặc đã bị vô hiệu hóa.";
    public const string AccountLocked = "Tài khoản đang bị khóa.";
    public const string ActiveActorRequired = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.ActiveActorRequired;
    public const string ActorNotFound = "Không tìm thấy tài khoản thực hiện thao tác.";
    public const string AuthenticationRequired = "Vui lòng đăng nhập để tiếp tục.";
    public const string AvatarInvalid = "Đường dẫn ảnh đại diện không hợp lệ.";
    public static string AvatarTooLarge => $"Ảnh đại diện không được vượt quá {AuthLimits.AvatarMaxSizeMegabytes} MB.";
    public const string AvatarTypeUnsupported = "Ảnh đại diện chỉ hỗ trợ định dạng PNG, JPEG và WebP.";
    public const string BirthDateInFuture = "Ngày sinh không được ở tương lai.";
    public const string CannotCreateTechnicalAdmin = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.CannotCreateTechnicalAdmin;
    public const string CannotManageAccount = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.CannotManageAccount;
    public const string CannotManageAdmin = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.CannotManageAdmin;
    public const string CannotManageSelf = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.CannotManageSelf;
    public const string CurrentPasswordIncorrect = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.CurrentPasswordIncorrect;
    public const string DataConflict = "Email, vai trò hoặc mã xác nhận đã tồn tại. Vui lòng thử lại.";
    public const string EmailAlreadyRegistered = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.EmailAlreadyRegistered;
    public const string EmailInputInvalid = "Địa chỉ email không hợp lệ.";
    public const string FullNameInvalid = "Họ và tên không hợp lệ.";
    public const string IdentityInvalid = "Thông tin xác thực không hợp lệ.";
    public const string LockExpiryInPast = "Thời điểm hết hạn khóa phải ở tương lai.";
    public const string LoginFailed = "Email hoặc mật khẩu không chính xác, hoặc tài khoản chưa xác nhận email.";
    public const string OtpInvalid = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.OtpInvalid;
    public const string PasswordConfirmationMismatch = "Mật khẩu xác nhận không khớp.";
    public const string PasswordUnchanged = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.PasswordUnchanged;
    public const string PermissionIdsMustBeDistinct = "Các mã quyền không được trùng nhau.";
    public const string PermissionUnknown = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.PermissionUnknown;
    public const string RateLimitExceeded = "Bạn đã thao tác quá nhiều lần. Vui lòng thử lại sau.";
    public const string RoleNotFound = "Không tìm thấy vai trò.";
    public const string RoleUnknown = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.RoleUnknown;
    public const string RolesInvalid = "Vui lòng chọn các vai trò hợp lệ và không trùng nhau.";
    public const string RolesMustBeDistinct = "Vui lòng chọn các vai trò không trùng nhau.";
    public const string StaffCanOnlyManageCustomers = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.StaffCanOnlyManageCustomers;
    public const string TechnicalAdminPermissionsProtected = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.TechnicalAdminPermissionsProtected;
    public const string TechnicalAdminRequired = global::AuthModule.Domain.Exceptions.DomainExceptionMessage.TechnicalAdminRequired;
    public const string UserNotFound = "Không tìm thấy tài khoản.";
}
