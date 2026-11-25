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

namespace AuthModule.Application.UseCases.ResetPassword;
internal sealed class ResetPassword_UCHandler : IRequestHandler<ResetPassword_UCInput, ResetPassword_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IAuthOtpManager _tokens;
    private readonly IPasswordHasher _hasher;
    private readonly IExecuteAtomically _transaction;
    private readonly IRefreshSessionRepository _sessions;

    public ResetPassword_UCHandler(IUserRepository users, IAuthOtpManager tokens, IPasswordHasher hasher, IExecuteAtomically transaction, IRefreshSessionRepository sessions)
    {
        _users = users;
        _tokens = tokens;
        _hasher = hasher;
        _transaction = transaction;
        _sessions = sessions;
    }

    public async Task<ResetPassword_UCOutput> Handle(ResetPassword_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            var user = await _users.GetUserByEmailAsync(input.Email, ct, forUpdate: true);
            user = AuthWorkflowRules.RequireResetEligibleUser(user);

            await _tokens.ConsumeOtpAsync(user.Id, OtpType.RESET_PASSWORD, input.Token, ct);
            var now = DateTimeOffset.UtcNow;
            user.ChangePassword(_hasher.HashPassword(input.NewPassword), now);
            await _users.UpdateAsync(user, ct);
            await _sessions.RevokeUserAsync(user.Id, DateTimeOffset.UtcNow, ct);

            return new ResetPassword_UCOutput(ResponseMessage.SuccessPasswordReset);
        }, ct);
    }
}
