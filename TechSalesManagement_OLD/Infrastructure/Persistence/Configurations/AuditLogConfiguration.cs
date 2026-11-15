using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogDbModel>
{
    public void Configure(EntityTypeBuilder<AuditLogDbModel> builder)
    {
        builder.ToTable("AuditLog");
        
        builder.HasKey(x => x.id);
        
        builder.Property(x => x.action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.table_name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.primary_key).IsRequired().HasMaxLength(100);
        
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.user)
            .WithMany(u => u.audit_logs)
            .HasForeignKey(x => x.user_id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
