using MediatR;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.GetProvinces;

internal sealed class GetProvinces_UCHandler : IRequestHandler<GetProvinces_UCInput, GetProvinces_UCOutput>
{
    private readonly ILocationRepository _locations;

    public GetProvinces_UCHandler(ILocationRepository locations)
    {
        _locations = locations;
    }

    public async Task<GetProvinces_UCOutput> Handle(GetProvinces_UCInput input, CancellationToken cancellationToken)
    {
        List<Province> provinces = await _locations.GetProvincesAsync(cancellationToken);
        List<GetProvinces_UCOutput.ProvinceItem> items = new List<GetProvinces_UCOutput.ProvinceItem>();
        foreach (Province province in provinces)
        {
            items.Add(new GetProvinces_UCOutput.ProvinceItem(province.Code, province.Name));
        }

        return new GetProvinces_UCOutput(items);
    }
}
