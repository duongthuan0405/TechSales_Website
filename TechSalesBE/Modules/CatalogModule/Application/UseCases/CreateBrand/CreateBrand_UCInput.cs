using MediatR;

namespace CatalogModule.Application.UseCases.CreateBrand;

internal sealed record CreateBrand_UCInput(string? Name) : IRequest<CreateBrand_UCOutput>;
