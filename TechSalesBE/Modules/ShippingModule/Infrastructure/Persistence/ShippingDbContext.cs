using ShippingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace ShippingModule.Infrastructure.Persistence;

public sealed class ShippingDbContext : DbContext
{
    internal DbSet<ProviceDbModel> Provices => Set<ProviceDbModel>();
    internal DbSet<WardDbModel> Wards => Set<WardDbModel>();
    internal DbSet<ShippingAddressDbModel> ShippingAddresss => Set<ShippingAddressDbModel>();
    internal DbSet<ShippingFeeDbModel> ShippingFees => Set<ShippingFeeDbModel>();

    public const string Schema = "shipping";

    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ShippingDbContext).Assembly,
            type => type.Namespace == "ShippingModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("ShippingModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}