using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.Services.Strategies;
using PaymentRefundModule.Infrastructure.Services.Strategies;
using PaymentRefundModule.Infrastructure.Services;
using PaymentRefundModule.Infrastructure.Repositories;
using PaymentRefundModule.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using PaymentRefundModule.Presentation.Contracts.CreatePayment;
using PaymentRefundModule.Presentation.Contracts.ReplacePayment;

namespace PaymentRefundModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{

    public static IServiceCollection RegisterModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPaymentRefundPersistence(configuration);
        services.AddScoped<ITransactionMethodRepository, TransactionMethodRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<PaymentOwnershipService>();
        services.AddScoped<PaymentCreationService>();
        services.AddScoped<IPaymentGatewayStrategy, MomoPaymentStrategy>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IPaymentCreateContract, PaymentCreateContract>();
        services.AddScoped<IPaymentReplaceContract, PaymentReplaceContract>();
        services.AddScoped<IExecuteAtomically, ExecuteAtomically>();
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly);
        });
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        return services;
    }
}
