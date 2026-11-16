using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfterSaleModule.Infrastructure.Persistence.Configurations;

internal sealed class ReturnResultProductUnitConfiguration : IEntityTypeConfiguration<ReturnResultProductUnitDbModel>
{
    public void Configure(EntityTypeBuilder<ReturnResultProductUnitDbModel> builder)
    {
        builder.ToTable("ReturnResultProductUnit");
        builder.HasKey(e => new { e.ReturnResultId, e.ProductId, e.SerialNumber });
        builder.Property(e => e.ReturnResultId).HasColumnName("ReturnResultId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.SerialNumber).HasColumnName("SerialNumber").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.IsApproved).HasColumnName("IsApproved").HasColumnType("boolean").IsRequired();
        builder.Property(e => e.ResultCheckingDescription).HasColumnName("ResultCheckingDescription").HasColumnType("character varying(500)").HasMaxLength(500).IsRequired(false);
    }
}
