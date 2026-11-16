using AfterSaleModule.Domain.Enums;
using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfterSaleModule.Infrastructure.Persistence.Configurations;

internal sealed class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequestDbModel>
{
    public void Configure(EntityTypeBuilder<ReturnRequestDbModel> builder)
    {
        builder.ToTable("ReturnRequest");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.OrderId).HasColumnName("OrderId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Quantity).HasColumnName("Quantity").HasColumnType("integer").IsRequired().HasDefaultValue(1);
        builder.Property(e => e.ReasonId).HasColumnName("ReasonId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ReasonDescription).HasColumnName("ReasonDescription").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired(false);
        builder.HasOne(e => e.Reason).WithMany(e => e.ReturnRequests).HasForeignKey(e => e.ReasonId).OnDelete(DeleteBehavior.Restrict);
    }
}
