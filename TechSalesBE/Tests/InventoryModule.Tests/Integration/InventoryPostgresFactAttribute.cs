using Xunit;

namespace InventoryModule.Tests.Integration;

public sealed class InventoryPostgresFactAttribute : FactAttribute
{
    internal const string ConnectionVariable = "INVENTORY_TEST_POSTGRES";

    public InventoryPostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
        {
            Skip = $"Set {ConnectionVariable} to run against an isolated PostgreSQL test server.";
        }
    }
}
