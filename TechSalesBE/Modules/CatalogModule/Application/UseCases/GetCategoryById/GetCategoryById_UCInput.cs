using MediatR;

namespace CatalogModule.Application.UseCases.GetCategoryById;

internal sealed record GetCategoryById_UCInput(Guid Id) : IRequest<GetCategoryById_UCOutput>;
