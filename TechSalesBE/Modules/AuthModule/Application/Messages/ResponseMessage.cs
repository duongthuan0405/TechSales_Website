namespace AuthModule.Application.Messages;

public static class ResponseMessage
{
    public const string SuccessAccount = "Lấy thông tin tài khoản thành công.";
    public const string SuccessAccountCreated = "Tạo tài khoản thành công.";
    public const string SuccessAccountLocked = "Khóa tài khoản thành công.";
    public const string SuccessAccountUnlocked = "Mở khóa tài khoản thành công.";
    public const string SuccessAccountUpdated = "Cập nhật tài khoản thành công.";
    public const string SuccessCustomers = "Lấy danh sách khách hàng thành công.";
    public const string SuccessEmailVerified = "Xác nhận email thành công.";
    public const string SuccessLogin = "Đăng nhập thành công.";
    public const string SuccessLogout = "Đăng xuất thành công.";
    public const string SuccessPasswordChanged = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
    public const string SuccessPasswordReset = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.";
    public const string SuccessPermissions = "Lấy danh sách quyền thành công.";
    public const string SuccessProfileUpdated = "Cập nhật hồ sơ thành công.";
    public const string SuccessRegistered = "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản.";
    public const string SuccessResetRequested = "Nếu tài khoản tồn tại, mã đặt lại mật khẩu đã được gửi đến email của bạn.";
    public const string SuccessRolePermissionsUpdated = "Cập nhật quyền của vai trò thành công.";
    public const string SuccessRoles = "Lấy danh sách vai trò thành công.";
    public const string SuccessRolesUpdated = "Cập nhật vai trò tài khoản thành công.";
    public const string SuccessStaff = "Lấy danh sách nhân viên thành công.";
}
