using AuthModule.Application.Constants;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthModule.Infrastructure.Services;

internal sealed class AuthBootstrapService : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _configuration;

    public AuthBootstrapService(IServiceScopeFactory scopes, IConfiguration configuration)
    {
        _scopes = scopes;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string? email = _configuration["AUTH:Bootstrap:Email"];
        string? password = _configuration["AUTH:Bootstrap:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (email == null || password == null || !AuthRules.ValidEmail(email) || !AuthRules.ValidPassword(password))
        {
            throw new InvalidOperationException("Provide a valid AUTH__Bootstrap__Email and AUTH__Bootstrap__Password, or omit both.");
        }

        using IServiceScope scope = _scopes.CreateScope();
        IServiceProvider provider = scope.ServiceProvider;
        IUserRepository users = provider.GetRequiredService<IUserRepository>();
        IExecuteAtomically transaction = provider.GetRequiredService<IExecuteAtomically>();
        IRoleRepository roles = provider.GetRequiredService<IRoleRepository>();
        IPasswordHasher passwordHasher = provider.GetRequiredService<IPasswordHasher>();

        await transaction.ExecuteAtomicallyAsync(async () =>
        {
            User? existing = await users.GetUserByEmailAsync(email, cancellationToken, forUpdate: true);
            if (existing != null)
            {
                bool isTechnicalAdmin = existing.Roles.Any(role => role.Name == UserRoleName.TECHNICAL_ADMIN);
                if (existing.Status != UserStatus.ACTIVE || !isTechnicalAdmin)
                {
                    throw new InvalidOperationException("Bootstrap email belongs to an existing non-administrator or inactive account. It will not be promoted automatically.");
                }
                return true;
            }

            Role? technicalAdminRole = await roles.GetByNameAsync(UserRoleName.TECHNICAL_ADMIN, cancellationToken);
            if (technicalAdminRole == null)
            {
                throw new InvalidOperationException("Apply Auth migrations before bootstrapping the first Technical Admin.");
            }

            string passwordHash = passwordHasher.HashPassword(password);
            User user = User.Create(email, passwordHash, verified: true);
            user.Roles.Add(technicalAdminRole);
            await users.AddAsync(user, cancellationToken);
            return true;
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
