using System.Net.Mail;
using MailModule.Application.Interfaces;
using MailModule.Application.Services;
using MailModule.Infrastructure.Configurations;
using MailModule.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MailModule;

public static class DependencyInjection
{
    public static IServiceCollection AddMailModule(this IServiceCollection services,
        IConfiguration configuration, string sectionName = SmtpOptions.SectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services.AddMailModule(options => configuration.GetSection(sectionName).Bind(options));
    }

    public static IServiceCollection AddMailModule(this IServiceCollection services, Action<SmtpOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.AddOptions<SmtpOptions>().Configure(configure)
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "Mail:Host is required.")
            .Validate(o => o.Port is > 0 and <= 65535, "Mail:Port must be between 1 and 65535.")
            .Validate(o => MailAddress.TryCreate(o.FromEmail, out var parsed) && parsed.Address == o.FromEmail,
                "Mail:FromEmail must be a valid email address.")
            .Validate(o => Enum.IsDefined(o.Security), "Mail:Security is invalid.")
            .Validate(o => o.TimeoutMilliseconds > 0, "Mail:TimeoutMilliseconds must be positive.")
            .Validate(o => string.IsNullOrWhiteSpace(o.UserName) == string.IsNullOrWhiteSpace(o.Password),
                "Provide both Mail:UserName and Mail:Password, or neither.")
            .ValidateOnStart();
        services.TryAddTransient<IEmailTransport, SmtpEmailTransport>();
        services.TryAddTransient<IEmailService, EmailService>();
        return services;
    }
}
