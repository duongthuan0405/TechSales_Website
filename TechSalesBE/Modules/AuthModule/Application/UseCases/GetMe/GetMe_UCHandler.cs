using AuthModule.Application.Exceptions;
using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.GetMe;
internal sealed class GetMe_UCHandler : IRequestHandler<GetMe_UCInput, GetMe_UCOutput>
{
    private readonly IUserRepository _users;

    public GetMe_UCHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<GetMe_UCOutput> Handle(GetMe_UCInput input, CancellationToken ct)
    {
        User user = await _users.GetByIdAsync(input.ActorId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.UserNotFound);
        List<string> roleNames = new List<string>();
        foreach (Role role in user.Roles)
        {
            roleNames.Add(role.Name);
        }

        GetMe_UCOutput output = new GetMe_UCOutput
        {
            Id = user.Id,
            Email = user.Email,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            LockedUntil = user.LockedUntil,
            Roles = roleNames,
            Profile = new GetMe_UCOutput.ProfileData
            {
                FullName = user.Profile.FullName,
                Phone = user.Profile.PhoneNumber,
                AvatarUrl = user.Profile.AvatarUrl,
                DateOfBirth = user.Profile.DateOfBirth
            }
        };
        return output;
    }
}
