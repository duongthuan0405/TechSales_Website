using CatalogModule.Application.Services;
using MediatR;

namespace CatalogModule.Application.UseCases.UpdateProductImages;

internal sealed record UpdateProductImages_UCInput(Guid Id, List<Guid> RetainedImageIds,
    List<ProductImageUpload> Images, int? PrimaryImageIndex) : IRequest<UpdateProductImages_UCOutput>;
