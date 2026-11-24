using MediatR;

namespace CatalogModule.Application.UseCases.GetBrandById;

internal sealed record GetBrandById_UCInput(Guid Id) : IRequest<GetBrandById_UCOutput>;
