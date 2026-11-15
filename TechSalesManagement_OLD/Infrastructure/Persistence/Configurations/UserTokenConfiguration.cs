using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserTokenDbModel>
{
    public void Configure(EntityTypeBuilder<UserTokenDbModel> builder)
    {
        builder.ToTable("UserToken");
        builder.HasKey(x => x.id);
        
        builder.Property(x => x.token).IsRequired().HasMaxLength(500);
        builder.Property(x => x.type).HasConversion<string>().HasMaxLength(50);
        
        builder.HasIndex(x => new { x.user_id, x.type });
        builder.HasIndex(x => x.token).IsUnique();
        
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.user)
            .WithMany(u => u.user_tokens)
            .HasForeignKey(x => x.user_id);
    }
}
