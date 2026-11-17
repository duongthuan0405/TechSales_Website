using MediatR;
namespace AuthModule.Application.UseCases.UpdateProfile;
internal sealed record UpdateProfile_UCInput(Guid ActorId, string? FullName, string? Phone, string? AvatarUrl, DateOnly? DateOfBirth, Stream? AvatarContent = null, string? AvatarContentType = null, long AvatarLength = 0) : IRequest<UpdateProfile_UCOutput>;
