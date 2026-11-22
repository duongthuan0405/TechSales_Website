namespace ProductModule.Application.UseCases.CreateProduct;

public sealed record CreateProduct_UCOutput(Guid Id, string Name, string Description, long? Price,
    string Status, Guid BrandId, Guid CategoryId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    List<CreateProductImage_UCOutput> Images);

public sealed record CreateProductImage_UCOutput(Guid Id, string ImageUrl, bool IsPrimary);
