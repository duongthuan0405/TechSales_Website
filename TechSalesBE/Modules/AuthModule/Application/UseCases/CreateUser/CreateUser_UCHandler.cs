using AuthModule.Application.Exceptions;
using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.CreateUser;
internal sealed class CreateUser_UCHandler : IRequestHandler<CreateUser_UCInput, CreateUser_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _hasher;
    private readonly IExecuteAtomically _transaction;

    public CreateUser_UCHandler(IUserRepository users, IRoleRepository roles, IPasswordHasher hasher, IExecuteAtomically transaction)
    {
        _users = users;
        _roles = roles;
        _hasher = hasher;
        _transaction = transaction;
    }

    public async Task<CreateUser_UCOutput> Handle(CreateUser_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            User? actor = await _users.GetByIdAsync(input.ActorId, ct);
            if (actor == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ActorNotFound);
            }
            AccountManagementRules.EnsureCanCreate(actor, input.Roles);

            User? existingUser = await _users.GetUserByEmailAsync(input.Email, ct);
            AuthWorkflowRules.EnsureEmailAvailable(existingUser, null);
            if (await _users.PhoneExistsAsync(input.Phone, null, ct))
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.PhoneAlreadyUsed);

            var user = User.Create(input.Email, _hasher.HashPassword(input.Password), verified: true);
            user.Profile.Update(input.FullName, input.Phone, null, null, DateOnly.FromDateTime(DateTime.UtcNow));
            foreach (var name in input.Roles)
            {
                user.Roles.Add(await _roles.GetByNameAsync(name, ct) ?? throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.RoleUnknown));
            }

            await _users.AddAsync(user, ct);
            List<string> roleNames = new List<string>();
            foreach (Role role in user.Roles)
            {
                roleNames.Add(role.Name);
            }

            CreateUser_UCOutput output = new CreateUser_UCOutput
            {
                Id = user.Id,
                Email = user.Email,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                LockedUntil = user.LockedUntil,
                Roles = roleNames,
                Profile = new CreateUser_UCOutput.ProfileData
                {
                    FullName = user.Profile.FullName,
                    Phone = user.Profile.PhoneNumber,
                    AvatarUrl = user.Profile.AvatarUrl,
                    DateOfBirth = user.Profile.DateOfBirth
                }
            };
            return output;
        }, ct);
    }
}
