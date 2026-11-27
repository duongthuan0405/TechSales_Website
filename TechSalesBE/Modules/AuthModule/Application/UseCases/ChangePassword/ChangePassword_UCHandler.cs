using AuthModule.Application.Exceptions;
using AuthModule.Application.Messages;
using AuthModule.Domain.Rules;
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
    private readonly IRefreshSessionRepository _sessions;

    public ChangePassword_UCHandler(IUserRepository users, IUserOtpRepository tokens, IPasswordHasher hasher, IExecuteAtomically transaction, IRefreshSessionRepository sessions)
    {
        _users = users;
        _tokens = tokens;
        _hasher = hasher;
        _transaction = transaction;
        _sessions = sessions;
    }

    public async Task<ChangePassword_UCOutput> Handle(ChangePassword_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId], ct);
            User user = await _users.GetByIdAsync(input.ActorId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.UserNotFound);
            bool currentPasswordMatches = _hasher.VerifyPassword(input.CurrentPassword, user.Password);
            AuthWorkflowRules.EnsureCurrentPasswordMatches(currentPasswordMatches);

            bool newPasswordMatches = _hasher.VerifyPassword(input.NewPassword, user.Password);
            AuthWorkflowRules.EnsurePasswordChanged(newPasswordMatches);

            var now = DateTimeOffset.UtcNow;
            user.ChangePassword(_hasher.HashPassword(input.NewPassword), now);
            await _users.UpdateAsync(user, ct);
            await _sessions.RevokeUserAsync(user.Id, DateTimeOffset.UtcNow, ct);

            await _tokens.InvalidateAsync(user.Id, OtpType.RESET_PASSWORD, now, ct);
            return new ChangePassword_UCOutput(ResponseMessage.SuccessPasswordChanged);
        }, ct);
    }
}
