using AuthModule.Application.Constants;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using MediatR;

namespace AuthModule.Application.UseCases.GetCustomers;
internal sealed class GetCustomers_UCHandler : IRequestHandler<GetCustomers_UCInput, GetCustomers_UCOutput>
{
    private readonly IUserRepository _users;

    public GetCustomers_UCHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<GetCustomers_UCOutput> Handle(GetCustomers_UCInput input, CancellationToken ct)
    {
        List<string> roleNames = new List<string>
        {
            UserRoleName.CUSTOMER
        };
        (List<User> Items, int Count) page = await _users.GetPageAsync(roleNames, input.PageNumber, input.PageSize, ct, input.Keyword);
        List<GetCustomers_UCOutput.UserItem> items = new List<GetCustomers_UCOutput.UserItem>();
        foreach (User user in page.Items)
        {
            List<string> userRoleNames = new List<string>();
            foreach (Role role in user.Roles)
            {
                userRoleNames.Add(role.Name);
            }

            GetCustomers_UCOutput.UserItem item = new GetCustomers_UCOutput.UserItem
            {
                Id = user.Id,
                Email = user.Email,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                LockedUntil = user.LockedUntil,
                Roles = userRoleNames,
                Profile = new GetCustomers_UCOutput.ProfileData
                {
                    FullName = user.Profile.FullName,
                    Phone = user.Profile.PhoneNumber,
                    AvatarUrl = user.Profile.AvatarUrl,
                    DateOfBirth = user.Profile.DateOfBirth
                }
            };
            items.Add(item);
        }

        return new GetCustomers_UCOutput
        {
            Items = items,
            TotalCount = page.Count,
            PageNumber = input.PageNumber,
            PageSize = input.PageSize
        };
    }
}
