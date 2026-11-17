using ShippingModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using MediatR;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.GetWards;

internal sealed class GetWards_UCHandler : IRequestHandler<GetWards_UCInput, GetWards_UCOutput>
{
    private readonly ILocationRepository _locations;

    public GetWards_UCHandler(ILocationRepository locations)
    {
        _locations = locations;
    }

    public async Task<GetWards_UCOutput> Handle(GetWards_UCInput input, CancellationToken cancellationToken)
    {
        Province? province = await _locations.GetProvinceAsync(input.ProvinceCode, cancellationToken);
        if (province == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProvinceNotFound);
        }

        List<Ward> wards = await _locations.GetWardsAsync(input.ProvinceCode, cancellationToken);
        List<GetWards_UCOutput.WardItem> items = new List<GetWards_UCOutput.WardItem>();
        foreach (Ward ward in wards)
        {
            items.Add(new GetWards_UCOutput.WardItem(ward.Code, ward.Name, ward.Province.Code));
        }

        return new GetWards_UCOutput(items);
    }
}
