using AuthModule.Application.Exceptions;
using AuthModule.Application.Messages;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.DeactivateUser;

internal sealed class DeactivateUser_UCHandler : IRequestHandler<DeactivateUser_UCInput, DeactivateUser_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IExecuteAtomically _transaction;
    private readonly IRefreshSessionRepository _sessions;

    public DeactivateUser_UCHandler(IUserRepository users, IExecuteAtomically transaction, IRefreshSessionRepository sessions)
    {
        _users = users;
        _transaction = transaction;
        _sessions = sessions;
    }

    public async Task<DeactivateUser_UCOutput> Handle(DeactivateUser_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync(new Guid[] { input.ActorId, input.UserId }, cancellationToken);
            User actor = await _users.GetByIdAsync(input.ActorId, cancellationToken)
                ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ActorNotFound);
            User target = await _users.GetByIdAsync(input.UserId, cancellationToken)
                ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.UserNotFound);
            AccountManagementRules.EnsureCanDeactivate(actor, target);
            target.Deactivate(DateTimeOffset.UtcNow);
            await _users.UpdateAsync(target, cancellationToken);
            await _sessions.RevokeUserAsync(target.Id, DateTimeOffset.UtcNow, cancellationToken);
            return new DeactivateUser_UCOutput(ResponseMessage.SuccessAccountDeactivated);
        }, cancellationToken);
    }
}
