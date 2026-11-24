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
            string? fullName = _configuration["AUTH:Bootstrap:FullName"];
            string? phone = _configuration["AUTH:Bootstrap:Phone"];
            if (!AuthRules.ValidFullName(fullName) || !AuthRules.ValidPhone(phone))
                throw new InvalidOperationException("Provide AUTH__Bootstrap__FullName and a valid AUTH__Bootstrap__Phone for the initial administrator profile.");
            if (await users.PhoneExistsAsync(phone!, null, cancellationToken))
                throw new InvalidOperationException("Bootstrap phone number is already used.");
            user.Profile.Update(fullName, phone, null, null, DateOnly.FromDateTime(DateTime.UtcNow));
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
