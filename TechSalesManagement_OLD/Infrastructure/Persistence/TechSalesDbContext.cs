using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence;

public class TechSalesDbContext : DbContext
{
    public TechSalesDbContext(DbContextOptions<TechSalesDbContext> options) : base(options)
    {
    }

    // Identity Group
    public DbSet<UserDbModel> Users { get; set; }
    public DbSet<UserProfileDbModel> UserProfiles { get; set; }
    public DbSet<UserTokenDbModel> UserTokens { get; set; }
    public DbSet<RoleDbModel> Roles { get; set; }
    public DbSet<PermissionDbModel> Permissions { get; set; }
    public DbSet<UserRoleDbModel> UserRoles { get; set; }
    public DbSet<RolePermissionDbModel> RolePermissions { get; set; }
    public DbSet<ShippingAddressDbModel> ShippingAddresses { get; set; }

    // Product Group
    public DbSet<CategoryDbModel> Categories { get; set; }
    public DbSet<ProductDbModel> Products { get; set; }
    public DbSet<ProductImageDbModel> ProductImages { get; set; }
    public DbSet<InventoryDbModel> Inventories { get; set; }

    // Sales Group
    public DbSet<CartDbModel> Carts { get; set; }
    public DbSet<CartItemDbModel> CartItems { get; set; }
    public DbSet<OrderDbModel> Orders { get; set; }
    public DbSet<OrderItemDbModel> OrderItems { get; set; }
    public DbSet<VoucherDbModel> Vouchers { get; set; }
    public DbSet<OrderVoucherDbModel> OrderVouchers { get; set; }

    // Payment Group
    public DbSet<PaymentMethodDbModel> PaymentMethods { get; set; }
    public DbSet<PaymentDbModel> Payments { get; set; }

    // Engagement Group
    public DbSet<ReviewDbModel> Reviews { get; set; }
    public DbSet<ReviewResponseDbModel> ReviewResponses { get; set; }
    public DbSet<NotificationDbModel> Notifications { get; set; }
    public DbSet<AuditLogDbModel> AuditLogs { get; set; }

    // Content & Configuration Group
    public DbSet<ArticleDbModel> Articles { get; set; }
    public DbSet<SystemSettingDbModel> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nạp cấu hình từ Assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TechSalesDbContext).Assembly);
        
        // PostgreSQL Specific: UTC Timestamps & Enum to String conversion
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.GetProperties();
            
            // Cấu hình DateTimeOffset -> timestamp with time zone & ép về UTC (+0)
            var dateTimeOffsetConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, DateTimeOffset>(
                v => v.ToUniversalTime(),
                v => v);

        }
    }
}
