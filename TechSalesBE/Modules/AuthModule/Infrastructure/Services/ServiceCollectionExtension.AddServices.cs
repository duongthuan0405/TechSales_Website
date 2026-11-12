using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthModule.Application.Services;
using AuthModule.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Extensions
{
    internal static partial class ServiceCollectionExtension
    {
        internal static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IExecuteAtomically, ExecuteAtomically>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IOtpService, OtpService>();
            return services;
        }
    }
}