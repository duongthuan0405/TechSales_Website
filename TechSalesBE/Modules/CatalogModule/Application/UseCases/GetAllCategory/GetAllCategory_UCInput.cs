using MediatR;

namespace CatalogModule.Application.UseCases.GetAllCategory;

internal sealed record GetAllCategory_UCInput() : IRequest<GetAllCategory_UCOutput>;
