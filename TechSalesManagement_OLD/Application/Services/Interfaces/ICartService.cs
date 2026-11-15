using System.Threading.Tasks;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface ICartService
{
    Task AddToCartAsync(AddToCartParams parameters);
    Task UpdateCartItemAsync(UpdateCartItemParams parameters);
    Task RemoveCartItemAsync(RemoveCartItemParams parameters);
    Task<Cart> GetCartAsync(GetCartParams parameters);
}
