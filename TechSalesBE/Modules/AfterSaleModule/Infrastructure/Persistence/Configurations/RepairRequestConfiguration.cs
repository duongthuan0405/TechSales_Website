using AfterSaleModule.Domain.Enums;
using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfterSaleModule.Infrastructure.Persistence.Configurations;

internal sealed class RepairRequestConfiguration : IEntityTypeConfiguration<RepairRequestDbModel>
{
    public void Configure(EntityTypeBuilder<RepairRequestDbModel> builder)
    {
        builder.ToTable("RepairRequest");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.SerialNumber).HasColumnName("SerialNumber").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CustomerId).HasColumnName("CustomerId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired().HasDefaultValue(RepairRequestStatus.PENDING);
        builder.Property(e => e.ProblemDescription).HasColumnName("ProblemDescription").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.ReceivedDeviceAt).HasColumnName("ReceivedDeviceAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
    }
}
