using AuthModule.Application.Constants;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using MediatR;

namespace AuthModule.Application.UseCases.GetStaff;
internal sealed class GetStaff_UCHandler : IRequestHandler<GetStaff_UCInput, GetStaff_UCOutput>
{
    private readonly IUserRepository _users;

    public GetStaff_UCHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<GetStaff_UCOutput> Handle(GetStaff_UCInput input, CancellationToken ct)
    {
        (List<User> Items, int Count) page = await _users.GetPageAsync(input.RoleNames, input.PageNumber, input.PageSize, ct);
        List<GetStaff_UCOutput.UserItem> items = new List<GetStaff_UCOutput.UserItem>();
        foreach (User user in page.Items)
        {
            List<string> userRoleNames = new List<string>();
            foreach (Role role in user.Roles)
            {
                userRoleNames.Add(role.Name);
            }

            GetStaff_UCOutput.UserItem item = new GetStaff_UCOutput.UserItem
            {
                Id = user.Id,
                Email = user.Email,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                LockedUntil = user.LockedUntil,
                Roles = userRoleNames,
                Profile = new GetStaff_UCOutput.ProfileData
                {
                    FullName = user.Profile.FullName,
                    Phone = user.Profile.PhoneNumber,
                    AvatarUrl = user.Profile.AvatarUrl,
                    DateOfBirth = user.Profile.DateOfBirth
                }
            };
            items.Add(item);
        }

        return new GetStaff_UCOutput
        {
            Items = items,
            TotalCount = page.Count,
            PageNumber = input.PageNumber,
            PageSize = input.PageSize
        };
    }
}
