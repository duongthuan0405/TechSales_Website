using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaymentRefundModule.Infrastructure.Persistence.Configurations;

internal sealed class TransactionMethodConfiguration : IEntityTypeConfiguration<TransactionMethodDbModel>
{
    public void Configure(EntityTypeBuilder<TransactionMethodDbModel> builder)
    {
        builder.ToTable("TransactionMethod");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired(false);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.Type).HasColumnName("Type").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired(false);
    }
}
