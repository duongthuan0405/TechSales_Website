namespace ProductModule.Application.UseCases.UpdateProduct;

public sealed record UpdateProduct_UCOutput(Guid Id, string Name, string Description, long? Price,
    string Status, Guid BrandId, Guid CategoryId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    List<UpdateProductImage_UCOutput> Images);

public sealed record UpdateProductImage_UCOutput(Guid Id, string ImageUrl, bool IsPrimary);
