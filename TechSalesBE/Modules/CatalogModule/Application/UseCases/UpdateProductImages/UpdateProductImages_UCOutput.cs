namespace CatalogModule.Application.UseCases.UpdateProductImages;

public sealed record UpdateProductImages_UCOutput(Guid Id, List<UpdatedProductImage> Images);
public sealed record UpdatedProductImage(Guid Id, string ImageUrl, bool IsPrimary);
