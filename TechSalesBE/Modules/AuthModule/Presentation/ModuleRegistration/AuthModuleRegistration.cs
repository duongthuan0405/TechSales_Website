using AuthModule.Extensions;
using AuthModule.Infrastructure.AuthN;
using AuthModule.Infrastructure.AuthZ;
using AuthModule.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Presentation.ModuleRegistration
{
    public static partial class ServiceCollectionExtension
    {
        public static IServiceCollection RegisterAuthModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddServices();
            services.AddRepositories();
            services.AddControllers();
            services.AddAuthN(configuration);
            //services.AddAuthZ();
            
            return services;
        }
    }
}