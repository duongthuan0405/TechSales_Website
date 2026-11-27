namespace InventoryModule.Application.Constants;

internal static class StockImportLimits
{
    public const int MaxFileMegabytes = 5;
    public const long MaxFileBytes = MaxFileMegabytes * 1024L * 1024;
    public const long MaxRequestBytes = MaxFileBytes + 64 * 1024;
    public const int MaxDataRows = 5_000;
}
