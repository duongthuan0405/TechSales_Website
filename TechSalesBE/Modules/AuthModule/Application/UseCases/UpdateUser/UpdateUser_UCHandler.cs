using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.UpdateUser;
internal sealed class UpdateUser_UCHandler : IRequestHandler<UpdateUser_UCInput, UpdateUser_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IUserOtpRepository _tokens;
    private readonly IExecuteAtomically _transaction;

    public UpdateUser_UCHandler(IUserRepository users, IUserOtpRepository tokens, IExecuteAtomically transaction)
    {
        _users = users;
        _tokens = tokens;
        _transaction = transaction;
    }

    public async Task<UpdateUser_UCOutput> Handle(UpdateUser_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId, input.UserId], ct);
            var user = await _users.GetByIdAsync(input.UserId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, "User not found.");
            var existing = await _users.GetUserByEmailAsync(input.Email, ct);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, "Email is already registered.");
            }

            user.Email = AuthRules.NormalizeEmail(input.Email);
            var now = DateTimeOffset.UtcNow;
            user.UpdatedAt = now;
            await _users.UpdateAsync(user, ct);

            await _tokens.InvalidateAsync(user.Id, OtpType.RESET_PASSWORD, now, ct);
            await _tokens.InvalidateAsync(user.Id, OtpType.EMAIL_VERIFICATION, now, ct);
            List<string> roleNames = new List<string>();
            foreach (Role role in user.Roles)
            {
                roleNames.Add(role.Name);
            }

            UpdateUser_UCOutput output = new UpdateUser_UCOutput
            {
                Id = user.Id,
                Email = user.Email,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                LockedUntil = user.LockedUntil,
                Roles = roleNames,
                Profile = new UpdateUser_UCOutput.ProfileData
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
