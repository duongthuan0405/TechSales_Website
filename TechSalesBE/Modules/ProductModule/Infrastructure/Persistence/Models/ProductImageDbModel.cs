namespace ProductModule.Infrastructure.Persistence.Models;

internal sealed class ProductImageDbModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool? IsPrimary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ProductDbModel Product { get; set; } = null!;
}
