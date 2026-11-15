using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<ReviewDbModel>
{
    public void Configure(EntityTypeBuilder<ReviewDbModel> builder)
    {
        builder.ToTable("Review");
        builder.HasKey(x => x.id);
        builder.Property(x => x.comment).HasMaxLength(2000);
        builder.Property(x => x.status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(x => x.product_id);
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.user)
            .WithMany(u => u.reviews)
            .HasForeignKey(x => x.user_id)
            .OnDelete(DeleteBehavior.SetNull); // Handle optional null cascade

        builder.HasOne(x => x.product)
            .WithMany(p => p.reviews)
            .HasForeignKey(x => x.product_id);
    }
}
