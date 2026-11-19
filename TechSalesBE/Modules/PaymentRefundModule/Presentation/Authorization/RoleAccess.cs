namespace PaymentRefundModule.Presentation.Authorization;

internal static class RoleAccess
{
    public const string Staff = "Staff";
    public const string BusinessAdmin = "Business Admin";
    public const string TechnicalAdmin = "Technical Admin";
    public const string PaymentManagement = Staff + "," + BusinessAdmin + "," + TechnicalAdmin;
}
