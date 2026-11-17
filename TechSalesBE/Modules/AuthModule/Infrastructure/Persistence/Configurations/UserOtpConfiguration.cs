using AuthModule.Domain.Enums;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthModule.Infrastructure.Persistence.Configurations;

internal sealed class UserOtpConfiguration : IEntityTypeConfiguration<UserOtpDbModel>
{
    public void Configure(EntityTypeBuilder<UserOtpDbModel> builder)
    {
        builder.ToTable("UserOtp");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Value).HasColumnName("Value").HasColumnType("character varying(500)").HasMaxLength(500).IsRequired();
        builder.Property(e => e.Type).HasColumnName("Type").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.ExpiredAt).HasColumnName("ExpiredAt").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(e => e.UsedAt).HasColumnName("UsedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.HasIndex(e => new { e.UserId, e.Type }).IsUnique();
        builder.HasOne(e => e.User).WithMany(e => e.Otps).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
