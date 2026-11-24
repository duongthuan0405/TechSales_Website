using AuthModule.Application.Exceptions;
using AuthModule.Application.Messages;
using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.UpdateProfile;
internal sealed class UpdateProfile_UCHandler : IRequestHandler<UpdateProfile_UCInput, UpdateProfile_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IAvatarStorage _avatars;
    private readonly IExecuteAtomically _transaction;

    public UpdateProfile_UCHandler(IUserRepository users, IAvatarStorage avatars, IExecuteAtomically transaction)
    {
        _users = users;
        _avatars = avatars;
        _transaction = transaction;
    }

    public async Task<UpdateProfile_UCOutput> Handle(UpdateProfile_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId], ct);
            User user = await _users.GetByIdAsync(input.ActorId, ct) ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.UserNotFound);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            // Validate all profile fields before writing a file.
            user.Profile.Update(input.FullName, input.Phone, input.AvatarUrl, input.DateOfBirth, today);
            if (await _users.PhoneExistsAsync(user.Profile.PhoneNumber, user.Id, ct))
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.PhoneAlreadyUsed);
            if (input.AvatarContent is not null)
            {
                user.Profile.Update(null, null, await _avatars.SaveAsync(input.AvatarContent, input.AvatarContentType ?? "", input.AvatarLength, ct), null, today);
            }

            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _users.UpdateAsync(user, ct);
            return new UpdateProfile_UCOutput(ResponseMessage.SuccessProfileUpdated);
        }, ct);
    }
}
