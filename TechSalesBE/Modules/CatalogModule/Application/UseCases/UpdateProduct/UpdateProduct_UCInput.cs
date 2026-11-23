using MediatR;

namespace CatalogModule.Application.UseCases.UpdateProduct;

internal sealed record UpdateProduct_UCInput(Guid Id, string Name, string Description, long? Price,
    Guid BrandId, Guid CategoryId, string Status) : IRequest<UpdateProduct_UCOutput>;
