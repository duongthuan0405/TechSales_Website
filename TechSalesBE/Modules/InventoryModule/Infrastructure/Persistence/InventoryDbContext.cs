using InventoryModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryModule.Infrastructure.Persistence;

public sealed class InventoryDbContext : DbContext
{
    internal DbSet<ProductUnitDbModel> ProductUnits => Set<ProductUnitDbModel>();
    internal DbSet<InvetoryDbModel> Invetorys => Set<InvetoryDbModel>();

    public const string Schema = "inventory";

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InventoryDbContext).Assembly,
            type => type.Namespace == "InventoryModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("InventoryModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}