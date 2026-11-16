using AfterSaleModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfterSaleModule.Infrastructure.Persistence.Configurations;

internal sealed class ReturnRequestReasonConfiguration : IEntityTypeConfiguration<ReturnRequestReasonDbModel>
{
    public void Configure(EntityTypeBuilder<ReturnRequestReasonDbModel> builder)
    {
        builder.ToTable("ReturnRequestReason");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(150)").HasMaxLength(150).IsRequired();
        builder.Property(e => e.Description).HasColumnName("Description").HasColumnType("character varying(500)").HasMaxLength(500).IsRequired(false);
    }
}
