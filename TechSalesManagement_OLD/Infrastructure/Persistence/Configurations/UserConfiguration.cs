using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserDbModel>
{
    public void Configure(EntityTypeBuilder<UserDbModel> builder)
    {
        builder.ToTable("User");

        builder.HasKey(x => x.id);
        
        builder.Property(x => x.email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.HasIndex(x => x.email).IsUnique();

        builder.Property(x => x.password)
            .IsRequired();

        builder.Property(x => x.status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(x => x.failed_login_attempts)
            .HasDefaultValue(0);

        builder.Property(x => x.created_at)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");
    }
}
