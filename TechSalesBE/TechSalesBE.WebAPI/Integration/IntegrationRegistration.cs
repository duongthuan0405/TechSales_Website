using AuthModule.Application.Ports;
using PaymentRefundModule.Application.Ports;

namespace TechSalesBE.WebAPI.Integration;

internal static class IntegrationRegistration
{
    internal static IServiceCollection RegisterModule(IServiceCollection services)
    {
        services.AddScoped<IAuthEmailAdapter, AuthEmailAdapter>();
        services.AddScoped<CatalogModule.Application.Ports.IProductRatings, ProductRatingsAdapter>();
        services.AddScoped<IOrderOwnershipPort, OrderOwnershipAdapter>();
        services.AddScoped<IRepairOwnershipPort, RepairOwnershipAdapter>();
        services.AddScoped<IPaymentAmountPort, PaymentAmountAdapter>();
        services.AddScoped<CartModule.Application.Ports.ICartProducts, CartProductsAdapter>();
        services.AddScoped<InventoryModule.Application.Ports.IInventoryProducts, InventoryProductsAdapter>();
        services.AddScoped<CatalogModule.Application.Ports.IProductInventory, ProductInventoryAdapter>();
        return services;
    }
}
