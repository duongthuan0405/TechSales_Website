namespace ShippingModule.Application.Services;

internal interface IShippingAddressLockService
{
    Task LockByUserAsync(Guid userId, CancellationToken cancellationToken);
}
