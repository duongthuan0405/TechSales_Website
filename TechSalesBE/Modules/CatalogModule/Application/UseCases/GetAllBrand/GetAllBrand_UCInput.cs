using MediatR;

namespace CatalogModule.Application.UseCases.GetAllBrand;

internal sealed record GetAllBrand_UCInput() : IRequest<GetAllBrand_UCOutput>;
