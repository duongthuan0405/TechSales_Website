using AuthModule.Infrastructure.Persistence;
using AuthModule.Extensions;
using AuthModule.Infrastructure.AuthN;
using AuthModule.Infrastructure.AuthZ;
using AuthModule.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Presentation.ModuleRegistration
{
    public static class ModuleRegistration
    {

        public static IServiceCollection RegisterModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthPersistence(configuration);
            services.AddServices();
            services.AddRepositories();
            services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
            services.AddAuthN(configuration);
            services.AddAuthorization();
            // Enable the permission policy provider when permission checks are implemented.
            //services.AddAuthZ();
            
            return services;
        }
    }
}