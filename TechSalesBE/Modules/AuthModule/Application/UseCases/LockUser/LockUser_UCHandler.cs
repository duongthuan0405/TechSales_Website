using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.LockUser;
internal sealed class LockUser_UCHandler : IRequestHandler<LockUser_UCInput, LockUser_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IExecuteAtomically _transaction;

    public LockUser_UCHandler(IUserRepository users, IExecuteAtomically transaction)
    {
        _users = users;
        _transaction = transaction;
    }

    public async Task<LockUser_UCOutput> Handle(LockUser_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId, input.UserId], ct);
            var user = await _users.GetByIdAsync(input.UserId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, "User not found.");
            var now = DateTimeOffset.UtcNow;
            user.LockAccount(input.Until, now);
            await _users.UpdateAsync(user, ct);

            return new LockUser_UCOutput("Account locked successfully.");
        }, ct);
    }
}
