using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.Repositories;

internal interface ILocationRepository
{
    Task<List<Province>> GetProvincesAsync(CancellationToken cancellationToken);
    Task<Province?> GetProvinceAsync(string code, CancellationToken cancellationToken);
    Task<List<Ward>> GetWardsAsync(string provinceCode, CancellationToken cancellationToken);
    Task<Ward?> GetWardAsync(string code, CancellationToken cancellationToken);
}
