using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.ChangePassword;
internal sealed class ChangePassword_UCHandler : IRequestHandler<ChangePassword_UCInput, ChangePassword_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IUserOtpRepository _tokens;
    private readonly IPasswordHasher _hasher;
    private readonly IExecuteAtomically _transaction;

    public ChangePassword_UCHandler(IUserRepository users, IUserOtpRepository tokens, IPasswordHasher hasher, IExecuteAtomically transaction)
    {
        _users = users;
        _tokens = tokens;
        _hasher = hasher;
        _transaction = transaction;
    }

    public async Task<ChangePassword_UCOutput> Handle(ChangePassword_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId], ct);
            User user = await _users.GetByIdAsync(input.ActorId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, "User not found.");
            if (!_hasher.VerifyPassword(input.CurrentPassword, user.Password))
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "Current password is incorrect.");
            }

            if (_hasher.VerifyPassword(input.NewPassword, user.Password))
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "New password must differ from the current password.");
            }

            var now = DateTimeOffset.UtcNow;
            user.ChangePassword(_hasher.HashPassword(input.NewPassword), now);
            await _users.UpdateAsync(user, ct);

            await _tokens.InvalidateAsync(user.Id, OtpType.RESET_PASSWORD, now, ct);
            return new ChangePassword_UCOutput("Password changed successfully. Please sign in again.");
        }, ct);
    }
}
