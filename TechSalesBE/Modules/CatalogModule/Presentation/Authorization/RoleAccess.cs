namespace CatalogModule.Presentation.Authorization;

internal static class RoleAccess
{
    public const string Administrators = "Business Admin,Technical Admin";
    public const string Readers = "Staff," + Administrators;
}
