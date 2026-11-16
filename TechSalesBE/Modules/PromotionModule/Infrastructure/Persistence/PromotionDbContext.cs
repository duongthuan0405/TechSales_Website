using PromotionModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace PromotionModule.Infrastructure.Persistence;

public sealed class PromotionDbContext : DbContext
{
    internal DbSet<VoucherDbModel> Vouchers => Set<VoucherDbModel>();
    internal DbSet<OrderVoucherDbModel> OrderVouchers => Set<OrderVoucherDbModel>();

    public const string Schema = "promotion";

    public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PromotionDbContext).Assembly,
            type => type.Namespace == "PromotionModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("PromotionModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}