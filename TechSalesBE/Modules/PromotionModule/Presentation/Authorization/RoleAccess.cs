namespace PromotionModule.Presentation.Authorization;

internal static class RoleAccess
{
    public const string BusinessAdmin = "Business Admin";
    public const string TechnicalAdmin = "Technical Admin";
    public const string Administrators = BusinessAdmin + "," + TechnicalAdmin;
}
