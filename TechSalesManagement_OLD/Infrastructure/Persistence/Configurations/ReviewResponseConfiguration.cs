using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class ReviewResponseConfiguration : IEntityTypeConfiguration<ReviewResponseDbModel>
{
    public void Configure(EntityTypeBuilder<ReviewResponseDbModel> builder)
    {
        builder.ToTable("ReviewResponse");
        builder.HasKey(x => x.id);
        builder.Property(x => x.content).IsRequired();
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.review)
            .WithMany(r => r.review_responses)
            .HasForeignKey(x => x.review_id);

        builder.HasOne(x => x.user)
            .WithMany(u => u.review_responses)
            .HasForeignKey(x => x.user_id);
    }
}
