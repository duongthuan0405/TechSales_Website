using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaymentRefundModule.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentDbModel>
{
    public void Configure(EntityTypeBuilder<PaymentDbModel> builder)
    {
        builder.ToTable("Payment");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Type).HasColumnName("Type").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired(false);
        builder.Property(e => e.OrderId).HasColumnName("OrderId").HasColumnType("uuid").IsRequired(false);
        builder.Property(e => e.RepairRequestId).HasColumnName("RepairRequestId").HasColumnType("uuid").IsRequired(false);
        builder.Property(e => e.TransactionMethodId).HasColumnName("TransactionMethodId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.Amount).HasColumnName("Amount").HasColumnType("bigint").IsRequired().HasDefaultValue(0L);
        builder.Property(e => e.TransactionRef).HasColumnName("TransactionRef").HasColumnType("character varying(255)").HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.FailureReason).HasColumnName("FailureReason").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.CompletedAt).HasColumnName("CompletedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.HasOne(e => e.TransactionMethod).WithMany(e => e.Payments).HasForeignKey(e => e.TransactionMethodId).OnDelete(DeleteBehavior.Restrict);
    }
}
