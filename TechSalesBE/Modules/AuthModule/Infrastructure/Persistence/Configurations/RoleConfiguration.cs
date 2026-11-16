using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthModule.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<RoleDbModel>
{
    public void Configure(EntityTypeBuilder<RoleDbModel> builder)
    {
        builder.ToTable("Role");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(50)").HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.Description).HasColumnName("Description").HasColumnType("character varying(255)").HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
    }
}
