using InventoryModule.Contracts;
using InventoryModule.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using InventoryRegistration = InventoryModule.Presentation.ModuleRegistration.ModuleRegistration;

namespace ModuleContracts.Tests;

public sealed class InventoryQueryTests
{
    [Fact]
    public async Task Registered_inventory_contract_reads_available_and_reserved_quantities_in_one_batch()
    {
        await using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection).Options;
        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["INVENTORY:DbConnectionString"] = "Host=unused;Database=unused" }).Build();
        InventoryRegistration.RegisterModule(services, configuration);
        services.AddScoped(_ => new InventoryDbContext(options));
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        InventoryDbContext db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await db.Database.EnsureCreatedAsync();
        Guid id = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Invetory" ("ProductId", "Quantity", "ReservedQuantity") VALUES ({id}, 7, 3)
            """);
        IInventoryQuery query = scope.ServiceProvider.GetRequiredService<IInventoryQuery>();
        List<ProductStock> stocks = await query.GetStocksAsync(new List<Guid> { id, Guid.NewGuid() }, default);
        ProductStock stock = Assert.Single(stocks);
        Assert.Equal(id, stock.ProductId);
        Assert.Equal(7, stock.Quantity);
        Assert.Equal(3, stock.ReservedQuantity);
        Assert.Empty(await query.GetStocksAsync(new List<Guid>(), default));
    }
}
