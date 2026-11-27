using MediatR;

namespace CatalogModule.Application.UseCases.CreateCategory;

internal sealed record CreateCategory_UCInput(string? Name) : IRequest<CreateCategory_UCOutput>;
