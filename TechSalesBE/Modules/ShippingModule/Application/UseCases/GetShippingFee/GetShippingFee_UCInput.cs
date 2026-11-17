using MediatR;

namespace ShippingModule.Application.UseCases.GetShippingFee;

internal sealed record GetShippingFee_UCInput(string? ProvinceCode, string? WardCode)
    : IRequest<GetShippingFee_UCOutput>;
