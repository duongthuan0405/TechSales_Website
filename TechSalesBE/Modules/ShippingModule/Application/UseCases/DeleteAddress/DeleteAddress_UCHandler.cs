using MediatR;
using ShippingModule.Application.Services;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Rules;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.DeleteAddress;

internal sealed class DeleteAddress_UCHandler : IRequestHandler<DeleteAddress_UCInput, DeleteAddress_UCOutput>
{
    private readonly IShippingAddressRepository _addresses;
    private readonly IExecuteAtomically _transaction;
    private readonly IShippingAddressLockService _addressLock;

    public DeleteAddress_UCHandler(IShippingAddressRepository addresses, IExecuteAtomically transaction, IShippingAddressLockService addressLock)
    {
        _addresses = addresses;
        _transaction = transaction;
        _addressLock = addressLock;
    }

    public async Task<DeleteAddress_UCOutput> Handle(DeleteAddress_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _addressLock.LockByUserAsync(input.ActorId, cancellationToken);
            ShippingAddress? existing = await _addresses.GetActiveAsync(input.AddressId, cancellationToken);
            ShippingAddress address = AddressOwnershipRules.EnsureOwned(existing, input.ActorId);

            DateTimeOffset now = DateTimeOffset.UtcNow;
            bool wasDefault = address.IsDefault;
            address.Delete(now);
            await _addresses.SaveAsync(address, cancellationToken);

            List<ShippingAddress> addresses = await _addresses.GetActiveByUserAsync(input.ActorId, cancellationToken);
            ShippingAddress? replacement = AddressDefaultRules.SelectReplacement(wasDefault, address.Id, addresses, now);
            if (replacement != null)
            {
                await _addresses.SaveAsync(replacement, cancellationToken);
            }

            return new DeleteAddress_UCOutput(address.Id);
        }, cancellationToken);
    }
}
