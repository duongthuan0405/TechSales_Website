using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthModule.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<PermissionDbModel>
{
    public void Configure(EntityTypeBuilder<PermissionDbModel> builder)
    {
        builder.ToTable("Permission");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Code).HasColumnName("Code").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Module).HasColumnName("Module").HasColumnType("character varying(50)").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
    }
}
