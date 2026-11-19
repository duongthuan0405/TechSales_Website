using AuthModule.Application.Ports;
using PaymentRefundModule.Application.Ports;

namespace TechSalesBE.WebAPI.Integration;

internal static class IntegrationRegistration
{
    internal static IServiceCollection RegisterModule(IServiceCollection services)
    {
        services.AddScoped<IAuthEmailAdapter, AuthEmailAdapter>();
        services.AddScoped<IOrderOwnershipPort, OrderOwnershipAdapter>();
        services.AddScoped<IRepairOwnershipPort, RepairOwnershipAdapter>();
        services.AddScoped<IPaymentAmountPort, PaymentAmountAdapter>();
        return services;
    }
}
