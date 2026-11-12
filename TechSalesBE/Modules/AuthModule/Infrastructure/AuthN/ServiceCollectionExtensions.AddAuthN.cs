using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthModule.Infrastructure.AuthN
{
    internal static partial class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddAuthN(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigAuthJwtBearer(configuration);
            return services;
        }
    }
}
