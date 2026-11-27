using MediatR;
using CatalogModule.Application.Services;

namespace CatalogModule.Application.UseCases.CreateProduct;

internal sealed record CreateProduct_UCInput(string Name, string Description, long? Price,
    Guid BrandId, Guid CategoryId, List<ProductImageUpload>? Images = null,
    int? PrimaryImageIndex = null) : IRequest<CreateProduct_UCOutput>;
