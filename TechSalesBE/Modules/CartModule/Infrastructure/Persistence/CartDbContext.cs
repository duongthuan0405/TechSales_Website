using CartModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CartModule.Infrastructure.Persistence;

public sealed class CartDbContext : DbContext
{
    internal DbSet<CartDbModel> Carts => Set<CartDbModel>();
    internal DbSet<CartItemDbModel> CartItems => Set<CartItemDbModel>();

    public const string Schema = "cart";

    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CartDbContext).Assembly,
            type => type.Namespace == "CartModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("CartModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}