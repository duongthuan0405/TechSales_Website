using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfileDbModel>
{
    public void Configure(EntityTypeBuilder<UserProfileDbModel> builder)
    {
        builder.ToTable("UserProfile");
        
        builder.HasKey(x => x.user_id);
        
        builder.Property(x => x.full_name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.phone).IsRequired().HasMaxLength(20);
        
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.user)
            .WithOne(u => u.user_profile)
            .HasForeignKey<UserProfileDbModel>(x => x.user_id);
    }
}
