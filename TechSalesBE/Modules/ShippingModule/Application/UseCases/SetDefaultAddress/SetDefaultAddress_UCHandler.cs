using MediatR;
using ShippingModule.Application.Services;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Rules;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.SetDefaultAddress;

internal sealed class SetDefaultAddress_UCHandler : IRequestHandler<SetDefaultAddress_UCInput, SetDefaultAddress_UCOutput>
{
    private readonly IShippingAddressRepository _addresses;
    private readonly IExecuteAtomically _transaction;

    public SetDefaultAddress_UCHandler(IShippingAddressRepository addresses, IExecuteAtomically transaction)
    {
        _addresses = addresses;
        _transaction = transaction;
    }

    public async Task<SetDefaultAddress_UCOutput> Handle(SetDefaultAddress_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _addresses.LockByUserAsync(input.ActorId, cancellationToken);
            ShippingAddress? existing = await _addresses.GetActiveAsync(input.AddressId, cancellationToken);
            ShippingAddress address = AddressOwnershipRules.EnsureOwned(existing, input.ActorId);

            DateTimeOffset now = DateTimeOffset.UtcNow;
            List<ShippingAddress> addresses = await _addresses.GetActiveByUserAsync(input.ActorId, cancellationToken);
            List<ShippingAddress> changed = AddressDefaultRules.SelectDefault(addresses, address.Id, now);
            foreach (ShippingAddress item in changed)
            {
                await _addresses.SaveAsync(item, cancellationToken);
            }

            return new SetDefaultAddress_UCOutput(address.Id);
        }, cancellationToken);
    }
}
