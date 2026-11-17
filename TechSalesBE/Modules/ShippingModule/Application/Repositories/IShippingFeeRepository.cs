using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.Repositories;

internal interface IShippingFeeRepository
{
    Task SaveAsync(ShippingFee shippingFee, CancellationToken cancellationToken);
    Task<ShippingFee?> GetByProvinceAsync(string provinceCode, CancellationToken cancellationToken);
}
