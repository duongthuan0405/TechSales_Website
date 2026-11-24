using AuthModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using AuthModule.Application.Constants;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using MediatR;
using AuthModule.Domain.Rules;

namespace AuthModule.Application.UseCases.GetUsers;
internal sealed class GetUsers_UCHandler : IRequestHandler<GetUsers_UCInput, GetUsers_UCOutput>
{
    private readonly IUserRepository _users;

    public GetUsers_UCHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<GetUsers_UCOutput> Handle(GetUsers_UCInput input, CancellationToken ct)
    {
        User? actor = await _users.GetByIdAsync(input.ActorId, ct);
        if (actor == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ActorNotFound);
        }
        List<string> visibleRoles = AccountManagementRules.GetVisibleUserRoles(actor);
        List<string> filteredRoles = AccountManagementRules.FilterVisibleRoles(visibleRoles, input.Role);

        (List<User> Items, int Count) page = await _users.GetPageAsync(filteredRoles, input.PageNumber, input.PageSize, ct, input.Keyword);
        List<GetUsers_UCOutput.UserItem> items = new List<GetUsers_UCOutput.UserItem>();
        foreach (User user in page.Items)
        {
            List<string> userRoleNames = new List<string>();
            foreach (Role role in user.Roles)
            {
                userRoleNames.Add(role.Name);
            }

            GetUsers_UCOutput.UserItem item = new GetUsers_UCOutput.UserItem
            {
                Id = user.Id,
                Email = user.Email,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                LockedUntil = user.LockedUntil,
                Roles = userRoleNames,
                Profile = new GetUsers_UCOutput.ProfileData
                {
                    FullName = user.Profile.FullName,
                    Phone = user.Profile.PhoneNumber,
                    AvatarUrl = user.Profile.AvatarUrl,
                    DateOfBirth = user.Profile.DateOfBirth
                }
            };
            items.Add(item);
        }

        return new GetUsers_UCOutput
        {
            Items = items,
            TotalCount = page.Count,
            PageNumber = input.PageNumber,
            PageSize = input.PageSize
        };
    }
}
