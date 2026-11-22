using MediatR;

namespace CatalogModule.Application.UseCases.UpdateBrand;

internal sealed record UpdateBrand_UCInput(Guid Id, string? Name) : IRequest<UpdateBrand_UCOutput>;
