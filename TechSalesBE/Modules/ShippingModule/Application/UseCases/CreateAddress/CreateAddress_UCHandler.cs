using ShippingModule.Application.Exceptions;
using ShippingModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;
using ShippingModule.Application.Services;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.CreateAddress;

internal sealed class CreateAddress_UCHandler : IRequestHandler<CreateAddress_UCInput, CreateAddress_UCOutput>
{
    private readonly IShippingAddressRepository _addresses;
    private readonly IExecuteAtomically _transaction;
    private readonly ILocationRepository _locations;

    public CreateAddress_UCHandler(IShippingAddressRepository addresses, IExecuteAtomically transaction, ILocationRepository locations)
    {
        _addresses = addresses;
        _transaction = transaction;
        _locations = locations;
    }

    public async Task<CreateAddress_UCOutput> Handle(CreateAddress_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _addresses.LockByUserAsync(input.ActorId, cancellationToken);
            Ward? ward = await _locations.GetWardAsync(input.WardCode, cancellationToken);
            if (ward == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.WardNotFound);
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            List<ShippingAddress> existingAddresses = await _addresses.GetActiveByUserAsync(input.ActorId, cancellationToken);
            bool isDefault = AddressDefaultRules.ShouldBeDefault(existingAddresses);
            ShippingAddress address = new ShippingAddress(
                Guid.NewGuid(), input.ActorId, ward, input.Detail, isDefault, now);
            await _addresses.SaveAsync(address, cancellationToken);

            return new CreateAddress_UCOutput(
                address.Id, address.Ward.Province.Code, address.Ward.Province.Name,
                address.Ward.Code, address.Ward.Name, address.Detail, address.IsDefault,
                address.CreatedAt, address.UpdatedAt);
        }, cancellationToken);
    }
}
