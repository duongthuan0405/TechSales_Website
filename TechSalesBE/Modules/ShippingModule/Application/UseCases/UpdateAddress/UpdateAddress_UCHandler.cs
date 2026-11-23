using ShippingModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using MediatR;
using ShippingModule.Application.Services;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Rules;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.UpdateAddress;

internal sealed class UpdateAddress_UCHandler : IRequestHandler<UpdateAddress_UCInput, UpdateAddress_UCOutput>
{
    private readonly IShippingAddressRepository _addresses;
    private readonly IExecuteAtomically _transaction;
    private readonly IShippingAddressLockService _addressLock;
    private readonly ILocationRepository _locations;

    public UpdateAddress_UCHandler(IShippingAddressRepository addresses, IExecuteAtomically transaction, ILocationRepository locations, IShippingAddressLockService addressLock)
    {
        _addresses = addresses;
        _transaction = transaction;
        _addressLock = addressLock;
        _locations = locations;
    }

    public async Task<UpdateAddress_UCOutput> Handle(UpdateAddress_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _addressLock.LockByUserAsync(input.ActorId, cancellationToken);
            ShippingAddress? existing = await _addresses.GetActiveAsync(input.AddressId, cancellationToken);
            ShippingAddress address = AddressOwnershipRules.EnsureOwned(existing, input.ActorId);
            Ward? ward = await _locations.GetWardAsync(input.WardCode, cancellationToken);
            if (ward == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.WardNotFound);
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            address.Update(ward, input.Detail, now);
            await _addresses.SaveAsync(address, cancellationToken);

            return new UpdateAddress_UCOutput(
                address.Id, address.Ward.Province.Code, address.Ward.Province.Name,
                address.Ward.Code, address.Ward.Name, address.Detail, address.IsDefault,
                address.CreatedAt, address.UpdatedAt);
        }, cancellationToken);
    }
}
