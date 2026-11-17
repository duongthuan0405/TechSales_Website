using MediatR;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.GetAddresses;

internal sealed class GetAddresses_UCHandler : IRequestHandler<GetAddresses_UCInput, GetAddresses_UCOutput>
{
    private readonly IShippingAddressRepository _addresses;

    public GetAddresses_UCHandler(IShippingAddressRepository addresses)
    {
        _addresses = addresses;
    }

    public async Task<GetAddresses_UCOutput> Handle(GetAddresses_UCInput input, CancellationToken cancellationToken)
    {
        List<ShippingAddress> addresses = await _addresses.GetActiveByUserAsync(input.ActorId, cancellationToken);
        List<GetAddresses_UCOutput.AddressItem> items = new List<GetAddresses_UCOutput.AddressItem>();
        foreach (ShippingAddress address in addresses)
        {
            items.Add(new GetAddresses_UCOutput.AddressItem(
                address.Id, address.Ward.Province.Code, address.Ward.Province.Name,
                address.Ward.Code, address.Ward.Name, address.Detail, address.IsDefault,
                address.CreatedAt, address.UpdatedAt));
        }

        return new GetAddresses_UCOutput(items);
    }
}
