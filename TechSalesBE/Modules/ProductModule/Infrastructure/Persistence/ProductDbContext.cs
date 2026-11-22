using ProductModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductModule.Infrastructure.Persistence;

public sealed class ProductDbContext : DbContext
{
    internal DbSet<BrandDbModel> Brands => Set<BrandDbModel>();
    internal DbSet<CategoryDbModel> Categorys => Set<CategoryDbModel>();
    internal DbSet<ProductDbModel> Products => Set<ProductDbModel>();
    internal DbSet<ProductImageDbModel> ProductImages => Set<ProductImageDbModel>();

    public const string Schema = "catalog";

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProductDbContext).Assembly,
            type => type.Namespace == "ProductModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("ProductModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}