using MediatR;

namespace ShippingModule.Application.UseCases.GetProvinces;

internal sealed record GetProvinces_UCInput() : IRequest<GetProvinces_UCOutput>;
