using OrderingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderingModule.Infrastructure.Persistence;

public sealed class OrderingDbContext : DbContext
{
    internal DbSet<OrderDbModel> Orders => Set<OrderDbModel>();
    internal DbSet<OrderItemDbModel> OrderItems => Set<OrderItemDbModel>();
    internal DbSet<OrderItemUnitDbModel> OrderItemUnits => Set<OrderItemUnitDbModel>();

    public const string Schema = "ordering";

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderingDbContext).Assembly,
            type => type.Namespace == "OrderingModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("OrderingModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}