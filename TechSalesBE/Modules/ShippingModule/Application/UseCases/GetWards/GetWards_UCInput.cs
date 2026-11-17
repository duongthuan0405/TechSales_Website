using MediatR;

namespace ShippingModule.Application.UseCases.GetWards;

internal sealed record GetWards_UCInput(string ProvinceCode) : IRequest<GetWards_UCOutput>;
