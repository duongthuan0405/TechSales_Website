using AuthModule.Domain.Constants;

namespace AuthModule.Presentation.Authorization;

internal static class RoleAccess
{
    public const string Administrators = AccountRoleNames.BUSINESS_ADMIN + "," + AccountRoleNames.TECHNICAL_ADMIN;
    public const string CustomerManagement = AccountRoleNames.STAFF + "," + Administrators;
}
