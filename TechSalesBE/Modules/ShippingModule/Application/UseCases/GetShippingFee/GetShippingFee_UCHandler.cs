using ShippingModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using MediatR;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;
using ShippingModule.Domain.Rules;

namespace ShippingModule.Application.UseCases.GetShippingFee;

internal sealed class GetShippingFee_UCHandler : IRequestHandler<GetShippingFee_UCInput, GetShippingFee_UCOutput>
{
    private readonly ILocationRepository _locations;
    private readonly IShippingFeeRepository _fees;

    public GetShippingFee_UCHandler(ILocationRepository locations, IShippingFeeRepository fees)
    {
        _locations = locations;
        _fees = fees;
    }

    public async Task<GetShippingFee_UCOutput> Handle(GetShippingFee_UCInput input, CancellationToken cancellationToken)
    {
        string provinceCode;
        if (input.ProvinceCode != null)
        {
            Province? province = await _locations.GetProvinceAsync(input.ProvinceCode, cancellationToken);
            if (province == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProvinceNotFound);
            }

            provinceCode = province.Code;
        }
        else
        {
            Ward? ward = await _locations.GetWardAsync(input.WardCode!, cancellationToken);
            if (ward == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.WardNotFound);
            }

            provinceCode = ward.Province.Code;
        }

        ShippingFee? configuredFee = await _fees.GetByProvinceAsync(provinceCode, cancellationToken);
        ShippingFee shippingFee = DeliveryRules.RequireConfiguredFee(configuredFee);
        return new GetShippingFee_UCOutput(shippingFee.ProvinceCode, shippingFee.Fee);
    }
}
