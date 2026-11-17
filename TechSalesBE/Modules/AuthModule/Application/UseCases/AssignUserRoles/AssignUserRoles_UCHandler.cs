using AuthModule.Application.Exceptions;
using AuthModule.Application.Messages;
using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.AssignUserRoles;
internal sealed class AssignUserRoles_UCHandler : IRequestHandler<AssignUserRoles_UCInput, AssignUserRoles_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _userRoles;
    private readonly IExecuteAtomically _transaction;

    public AssignUserRoles_UCHandler(IUserRepository users, IRoleRepository roles, IUserRoleRepository userRoles, IExecuteAtomically transaction)
    {
        _users = users;
        _roles = roles;
        _userRoles = userRoles;
        _transaction = transaction;
    }

    public async Task<AssignUserRoles_UCOutput> Handle(AssignUserRoles_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId, input.UserId], ct);
            User? actor = await _users.GetByIdAsync(input.ActorId, ct);
            if (actor == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ActorNotFound);
            }
            var user = await _users.GetByIdAsync(input.UserId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.UserNotFound);
            AccountManagementRules.EnsureCanAssignRoles(actor, user.Id);
            var selected = await _roles.GetByIdsAsync(input.RoleIds, ct);
            AuthWorkflowRules.EnsureRolesKnown(selected.Count, input.RoleIds.Length);

            await _userRoles.ReplaceAsync(user.Id, input.RoleIds, ct);
            var now = DateTimeOffset.UtcNow;
            user.UpdatedAt = now;
            await _users.UpdateAsync(user, ct);

            return new AssignUserRoles_UCOutput(ResponseMessage.SuccessRolesUpdated);
        }, ct);
    }
}
