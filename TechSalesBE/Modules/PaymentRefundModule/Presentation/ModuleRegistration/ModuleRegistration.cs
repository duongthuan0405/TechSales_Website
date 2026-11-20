using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Infrastructure.Payments;
using PaymentRefundModule.Infrastructure.Payments.Momo;
using PaymentRefundModule.Infrastructure.Payments.Vnpay;
using PaymentRefundModule.Infrastructure.Services;
using PaymentRefundModule.Infrastructure.Repositories;
using PaymentRefundModule.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentValidation;
using PaymentRefundModule.Presentation.Contracts.CreatePayment;
using PaymentRefundModule.Presentation.Contracts.ReplacePayment;
using PaymentRefundModule.Application.UseCases.ReceiveMomoIpn;

namespace PaymentRefundModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{

    public static IServiceCollection RegisterModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPaymentRefundPersistence(configuration);
        services.AddOptions<MomoOptions>()
            .Bind(configuration.GetSection(MomoOptions.SectionName));
        services.AddOptions<VnpayOptions>()
            .Bind(configuration.GetSection(VnpayOptions.SectionName));
        services.AddScoped<ITransactionMethodRepository, TransactionMethodRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<PaymentOwnershipService>();
        services.AddScoped<PaymentCreationService>();
        services.AddOptions<MomoPollingOptions>()
            .Bind(configuration.GetSection(MomoPollingOptions.SectionName))
            .Validate(options => options.IntervalSeconds >= 5 && options.BatchSize >= 1
                && options.BatchSize <= 100 && options.RequestSpacingMilliseconds >= 100,
                "Invalid MoMo polling configuration.")
            .ValidateOnStart();
        services.AddScoped<IMomoPollingSource, MomoPollingSource>();
        services.AddHttpClient<IMomoStatusQuery, MomoStatusQuery>((serviceProvider, client) =>
        {
            MomoOptions options = serviceProvider.GetRequiredService<IOptions<MomoOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHostedService<MomoPollingWorker>();
        services.AddHttpClient<MomoPaymentStrategy>((serviceProvider, client) =>
        {
            MomoOptions options = serviceProvider.GetRequiredService<IOptions<MomoOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        });
        services.AddScoped<IPaymentGatewayStrategy>(serviceProvider =>
            serviceProvider.GetRequiredService<MomoPaymentStrategy>());
        services.AddScoped<IPaymentGatewayStrategy, VnpayPaymentStrategy>();
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
