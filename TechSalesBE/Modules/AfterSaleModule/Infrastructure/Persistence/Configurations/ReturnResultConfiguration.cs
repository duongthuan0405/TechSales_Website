using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfterSaleModule.Infrastructure.Persistence.Configurations;

internal sealed class ReturnResultConfiguration : IEntityTypeConfiguration<ReturnResultDbModel>
{
    public void Configure(EntityTypeBuilder<ReturnResultDbModel> builder)
    {
        builder.ToTable("ReturnResult");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ApprovedQuantity).HasColumnName("ApprovedQuantity").HasColumnType("integer").IsRequired().HasDefaultValue(0);
        builder.Property(e => e.ResultCheckingDescription).HasColumnName("ResultCheckingDescription").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.TotalRefundAmount).HasColumnName("TotalRefundAmount").HasColumnType("bigint").IsRequired();
        builder.HasOne(e => e.ReturnRequest).WithOne(e => e.Result).HasForeignKey<ReturnResultDbModel>(e => e.Id).OnDelete(DeleteBehavior.Restrict);
    }
}
