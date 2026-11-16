using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AfterSaleModule.Infrastructure.Persistence;

public sealed class AfterSaleDbContext : DbContext
{
    internal DbSet<ReturnRequestReasonDbModel> ReturnRequestReasons => Set<ReturnRequestReasonDbModel>();
    internal DbSet<ReturnRequestDbModel> ReturnRequests => Set<ReturnRequestDbModel>();
    internal DbSet<ReturnRequestImageDbModel> ReturnRequestImages => Set<ReturnRequestImageDbModel>();
    internal DbSet<ReturnResultDbModel> ReturnResults => Set<ReturnResultDbModel>();
    internal DbSet<ReturnResultProductUnitDbModel> ReturnResultProductUnits => Set<ReturnResultProductUnitDbModel>();
    internal DbSet<RepairRequestDbModel> RepairRequests => Set<RepairRequestDbModel>();
    internal DbSet<RepairResultDbModel> RepairResults => Set<RepairResultDbModel>();

    public const string Schema = "after_sale";

    public AfterSaleDbContext(DbContextOptions<AfterSaleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AfterSaleDbContext).Assembly,
            type => type.Namespace == "AfterSaleModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("AfterSaleModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}