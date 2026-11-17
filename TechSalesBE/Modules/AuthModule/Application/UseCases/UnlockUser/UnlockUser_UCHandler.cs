using AuthModule.Application.Rules;
using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.UnlockUser;
internal sealed class UnlockUser_UCHandler : IRequestHandler<UnlockUser_UCInput, UnlockUser_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IExecuteAtomically _transaction;

    public UnlockUser_UCHandler(IUserRepository users, IExecuteAtomically transaction)
    {
        _users = users;
        _transaction = transaction;
    }

    public async Task<UnlockUser_UCOutput> Handle(UnlockUser_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId, input.UserId], ct);
            User? actor = await _users.GetByIdAsync(input.ActorId, ct);
            if (actor == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, "Actor account not found.");
            }
            var user = await _users.GetByIdAsync(input.UserId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, "User not found.");
            AccountManagementRules.EnsureCanManage(actor, user);
            user.UnlockAccount(DateTimeOffset.UtcNow);
            await _users.UpdateAsync(user, ct);
            return new UnlockUser_UCOutput("Account unlocked successfully.");
        }, ct);
    }
}
