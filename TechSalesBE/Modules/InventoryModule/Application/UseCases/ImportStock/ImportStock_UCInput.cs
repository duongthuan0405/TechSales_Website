using MediatR;

namespace InventoryModule.Application.UseCases.ImportStock;

internal sealed record ImportStock_UCInput(Stream Content, string FileName, long Length)
    : IRequest<ImportStock_UCOutput>;
