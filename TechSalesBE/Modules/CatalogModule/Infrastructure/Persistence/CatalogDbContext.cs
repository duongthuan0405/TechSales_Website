using CatalogModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogModule.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext
{
    internal DbSet<BrandDbModel> Brands => Set<BrandDbModel>();
    internal DbSet<CategoryDbModel> Categorys => Set<CategoryDbModel>();
    internal DbSet<ProductDbModel> Products => Set<ProductDbModel>();
    internal DbSet<ProductImageDbModel> ProductImages => Set<ProductImageDbModel>();

    public const string Schema = "catalog";

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            type => type.Namespace == "CatalogModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("CatalogModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}