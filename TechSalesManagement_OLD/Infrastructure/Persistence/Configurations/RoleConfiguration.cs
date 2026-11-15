using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<RoleDbModel>
{
    public void Configure(EntityTypeBuilder<RoleDbModel> builder)
    {
        builder.ToTable("Role");
        builder.HasKey(x => x.id);
        builder.Property(x => x.name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.name).IsUnique();
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasData(
            new RoleDbModel 
            { 
                id = Guid.Parse("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a"), 
                name = "Technical Admin", 
                description = "Technical Administrator with full system access",
                created_at = new DateTimeOffset(2026, 5, 12, 0, 0, 0, TimeSpan.Zero)
            },
            new RoleDbModel 
            { 
                id = Guid.Parse("75595ed2-8e03-476c-a59c-864fbc57b1a9"), 
                name = "Customer", 
                description = "Default customer access",
                created_at = new DateTimeOffset(2026, 5, 12, 0, 0, 0, TimeSpan.Zero)
            },
            new RoleDbModel 
            { 
                id = Guid.Parse("8e2a0a54-e882-4174-ae34-32f299096d13"), 
                name = "Staff", 
                description = "Sales Staff member access",
                created_at = new DateTimeOffset(2026, 5, 12, 0, 0, 0, TimeSpan.Zero)
            },
            new RoleDbModel 
            { 
                id = Guid.Parse("c22cf7a1-67f6-479c-a3df-9504f8270fa6"), 
                name = "Business Admin", 
                description = "Business Administrator for management tasks",
                created_at = new DateTimeOffset(2026, 5, 12, 0, 0, 0, TimeSpan.Zero)
            }
        );
    }
}
