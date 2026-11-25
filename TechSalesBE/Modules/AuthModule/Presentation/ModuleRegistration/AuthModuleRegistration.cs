using CommonModule.BusinessExceptions;
using CommonModule.Presentation.Configuration;
using AuthModule.Application.Behaviors;
using AuthModule.Application.Exceptions;
using System.Threading.RateLimiting;
using CommonModule.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AuthModule.Application.Services;
using AuthModule.Extensions;
using AuthModule.Infrastructure.AuthN;
using AuthModule.Infrastructure.AuthZ;
using AuthModule.Infrastructure.Configurations;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Repositories;
using AuthModule.Infrastructure.Services;
using CommonModule.Presentation.ApiResponseModels;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace AuthModule.Presentation.ModuleRegistration;
public static class ModuleRegistration
{
    public static IServiceCollection RegisterModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddVietnameseResponses();
        services.AddAuthPersistence(configuration);
        services.AddServices();
        services.AddScoped<AuthModule.Application.Repositories.IRefreshSessionRepository, RefreshSessionRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthOtpManager,AuthOtpManager>();
        services.AddRepositories();
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(BusinessRuleExceptionBehavior<,>)));
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);
        services.AddHttpContextAccessor();
        services.AddAuthN(configuration);
        services.AddAuthZ();
        services.AddCors(options => options.AddPolicy("auth-frontend", policy =>
        {
            var frontend = configuration["AUTH:FrontendUrl"];
            if (Uri.TryCreate(frontend, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                policy.WithOrigins(uri.GetLeftPart(UriPartial.Authority)).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }));
        services.AddHostedService<AuthBootstrapService>();
        services.AddOptions<AuthRateLimitOptions>()
            .Bind(configuration.GetSection(AuthRateLimitOptions.SectionName))
            .Validate(settings => settings.PermitLimit > 0, "AUTH:RateLimit:PermitLimit must be greater than zero.")
            .Validate(settings => settings.WindowMinutes > 0, "AUTH:RateLimit:WindowMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth-public", context =>
            {
                IOptions<AuthRateLimitOptions> configuredOptions = context.RequestServices
                    .GetRequiredService<IOptions<AuthRateLimitOptions>>();
                AuthRateLimitOptions settings = configuredOptions.Value;

                string clientIp = "unknown";
                if (context.Connection.RemoteIpAddress != null)
                {
                    clientIp = context.Connection.RemoteIpAddress.ToString();
                }

                return RateLimitPartition.GetFixedWindowLimiter(clientIp, partitionKey =>
                {
                    FixedWindowRateLimiterOptions limiterOptions = new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.PermitLimit,
                        Window = TimeSpan.FromMinutes(settings.WindowMinutes),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    };
                    return limiterOptions;
                });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                ApiErrorResponse response = new ApiErrorResponse(BusinessExceptionMessage.RateLimitExceeded)
                {
                    Code = BusinessExceptionCode.TOO_MANY_REQUESTS
                };
                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };
        });
        return services;
    }
}
