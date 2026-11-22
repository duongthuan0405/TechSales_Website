namespace ProductModule.Infrastructure.Persistence.Models;

internal sealed class CategoryDbModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAat { get; set; }

    public ICollection<ProductDbModel> Products { get; set; } = new HashSet<ProductDbModel>();
}
