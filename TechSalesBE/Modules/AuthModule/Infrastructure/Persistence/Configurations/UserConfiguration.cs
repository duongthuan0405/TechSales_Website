using AuthModule.Domain.Enums;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthModule.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserDbModel>
{
    public void Configure(EntityTypeBuilder<UserDbModel> builder)
    {
        builder.ToTable("User");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Email).HasColumnName("Email").HasColumnType("character varying(255)").HasMaxLength(255).IsRequired();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.HashedPassword).HasColumnName("HashedPassword").HasColumnType("character varying(255)").HasMaxLength(255).IsRequired();
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.FailedLoginAttempts).HasColumnName("FailedLoginAttempts").HasColumnType("integer").IsRequired().HasDefaultValue(0);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.LockedUntil).HasColumnName("LockedUntil").HasColumnType("timestamp with time zone").IsRequired(false);
    }
}
