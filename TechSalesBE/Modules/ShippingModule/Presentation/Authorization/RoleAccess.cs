namespace ShippingModule.Presentation.Authorization;

internal static class RoleAccess
{
    public const string BusinessAdminRole = "Business Admin";
    public const string TechnicalAdminRole = "Technical Admin";
    public const string Administrators = BusinessAdminRole + "," + TechnicalAdminRole;
}
