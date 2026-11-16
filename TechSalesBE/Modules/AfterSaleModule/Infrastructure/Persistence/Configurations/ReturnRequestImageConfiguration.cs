using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfterSaleModule.Infrastructure.Persistence.Configurations;

internal sealed class ReturnRequestImageConfiguration : IEntityTypeConfiguration<ReturnRequestImageDbModel>
{
    public void Configure(EntityTypeBuilder<ReturnRequestImageDbModel> builder)
    {
        builder.ToTable("ReturnRequestImage");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ImageUrl).HasColumnName("ImageUrl").HasColumnType("character varying(500)").HasMaxLength(500).IsRequired();
        builder.Property(e => e.ReturnRequestId).HasColumnName("ReturnRequestId").HasColumnType("uuid").IsRequired();
        builder.HasOne(e => e.ReturnRequest).WithMany(e => e.Images).HasForeignKey(e => e.ReturnRequestId).OnDelete(DeleteBehavior.Restrict);
    }
}
