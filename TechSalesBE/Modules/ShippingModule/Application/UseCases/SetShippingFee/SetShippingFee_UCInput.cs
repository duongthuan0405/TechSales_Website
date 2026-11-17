using MediatR;

namespace ShippingModule.Application.UseCases.SetShippingFee;

internal sealed record SetShippingFee_UCInput(string ProvinceCode, long Fee) : IRequest<SetShippingFee_UCOutput>;
