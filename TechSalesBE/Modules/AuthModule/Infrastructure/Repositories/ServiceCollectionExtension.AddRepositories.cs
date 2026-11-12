using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthModule.Application.Repositories;
using AuthModule.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Infrastructure.Repositories
{
    internal static partial class ServiceCollectionExtension
    {
        internal static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
            return services;
        }
    }
}