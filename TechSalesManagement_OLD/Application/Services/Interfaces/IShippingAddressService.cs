using System.Threading.Tasks;
using TechSalesManagement.Application.Services.Params;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IShippingAddressService
{
    Task CreateAddressAsync(CreateAddressParams parameters);
    Task UpdateAddressAsync(UpdateAddressParams parameters);
    Task SetDefaultAddressAsync(SetDefaultAddressParams parameters);
    Task<System.Collections.Generic.List<TechSalesManagement.Domain.Entities.ShippingAddress>> GetAddressesByUserIdAsync(System.Guid userId);
    Task DeleteAddressAsync(DeleteAddressParams parameters);
}
