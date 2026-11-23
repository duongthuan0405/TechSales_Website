using CartModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace CartModule.Tests;

public sealed class CartMigrationTests
{
    private const string Initial = "20261116100919_InitialCartSchema";

    [Fact]
    public void Fresh_database_script_creates_each_cart_table_once()
    {
        using CartDbContext context = CreateContext();
        string script = context.GetService<IMigrator>().GenerateScript();

        Assert.Equal(1, Count(script, "CREATE TABLE cart.\"Cart\""));
        Assert.Equal(1, Count(script, "CREATE TABLE cart.\"CartItem\""));
        Assert.Contains(Initial, script);
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public void Updating_current_database_does_not_repeat_table_creation()
    {
        using CartDbContext context = CreateContext();
        string script = context.GetService<IMigrator>().GenerateScript(Initial);

        Assert.DoesNotContain("CREATE TABLE cart.\"Cart\"", script);
        Assert.DoesNotContain("CREATE TABLE cart.\"CartItem\"", script);
        Assert.DoesNotContain("DROP TABLE", script);
        Assert.DoesNotContain("ALTER TABLE", script);
    }

    private static CartDbContext CreateContext()
    {
        DbContextOptions<CartDbContext> options = new DbContextOptionsBuilder<CartDbContext>()
            .UseNpgsql("Host=unused;Database=unused;Username=unused", postgres =>
                postgres.MigrationsHistoryTable("__EFMigrationsHistory", CartDbContext.Schema))
            .Options;
        return new CartDbContext(options);
    }

    private static int Count(string text, string value)
    {
        return text.Split(value, StringSplitOptions.None).Length - 1;
    }
}
