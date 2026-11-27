using AuthModule.Domain.Constants;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthModule.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfileDbModel>
{
    public void Configure(EntityTypeBuilder<UserProfileDbModel> builder)
    {
        builder.ToTable("UserProfile");
        builder.HasKey(e => e.UserId);
        builder.HasIndex(e => e.PhoneNumber).IsUnique().HasDatabaseName("IX_UserProfile_PhoneNumber");
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.FullName).HasColumnName("FullName").HasColumnType($"character varying({AuthLimits.FullNameMaxLength})").HasMaxLength(AuthLimits.FullNameMaxLength).IsRequired();
        builder.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber").HasColumnType("character varying(20)").HasMaxLength(20).IsRequired();
        builder.Property(e => e.AvatarUrl).HasColumnName("AvatarUrl").HasColumnType($"character varying({AuthLimits.AvatarUrlMaxLength})").HasMaxLength(AuthLimits.AvatarUrlMaxLength).IsRequired(false);
        builder.Property(e => e.DateOfBirth).HasColumnName("DateOfBirth").HasColumnType("date").IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.HasOne(e => e.User).WithOne(e => e.Profile).HasForeignKey<UserProfileDbModel>(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
