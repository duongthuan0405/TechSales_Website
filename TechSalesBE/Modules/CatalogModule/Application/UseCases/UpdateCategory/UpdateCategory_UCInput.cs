using MediatR;

namespace CatalogModule.Application.UseCases.UpdateCategory;

internal sealed record UpdateCategory_UCInput(Guid Id, string? Name) : IRequest<UpdateCategory_UCOutput>;
