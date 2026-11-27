using Xunit;

namespace CartModule.Tests.Integration;

public sealed class CartPostgresFactAttribute : FactAttribute
{
    internal const string ConnectionVariable = "CART_TEST_POSTGRES";

    public CartPostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
        {
            Skip = $"Set {ConnectionVariable} to run the Cart HTTP tests against PostgreSQL.";
        }
    }
}
