namespace InventoryModule.Presentation.Authorization;

internal static class RoleAccess
{
    private const string Staff = "Staff";
    private const string BusinessAdmin = "Business Admin";
    private const string TechnicalAdmin = "Technical Admin";
    public const string StockManagers = Staff + "," + BusinessAdmin + "," + TechnicalAdmin;
}
