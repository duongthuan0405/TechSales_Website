using AfterSaleModule.Domain.Enums;
using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfterSaleModule.Infrastructure.Persistence.Configurations;

internal sealed class RepairResultConfiguration : IEntityTypeConfiguration<RepairResultDbModel>
{
    public void Configure(EntityTypeBuilder<RepairResultDbModel> builder)
    {
        builder.ToTable("RepairResult");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        // WARRANTY is zero; it must not be mistaken for an unset value and replaced by REPAIR.
        builder.Property(e => e.RepairType).HasColumnName("RepairType").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired().HasDefaultValue(RepairType.REPAIR).HasSentinel(RepairType.REPAIR);
        builder.Property(e => e.ProcessingLocation).HasColumnName("ProcessingLocation").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired(false);
        builder.Property(e => e.CheckingResult).HasColumnName("CheckingResult").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.SolutionDescription).HasColumnName("SolutionDescription").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.TransferredToManufactureAt).HasColumnName("TransferredToManufactureAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.ReturnedToStoreAt).HasColumnName("ReturnedToStoreAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.CompletedAt).HasColumnName("CompletedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.TotalAmount).HasColumnName("TotalAmount").HasColumnType("bigint").IsRequired().HasDefaultValue(0L);
        builder.HasOne(e => e.RepairRequest).WithOne(e => e.Result).HasForeignKey<RepairResultDbModel>(e => e.Id).OnDelete(DeleteBehavior.Restrict);
    }
}
