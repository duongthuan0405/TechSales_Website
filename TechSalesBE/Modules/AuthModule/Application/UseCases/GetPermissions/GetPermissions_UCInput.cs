using MediatR;
namespace AuthModule.Application.UseCases.GetPermissions;
internal sealed record GetPermissions_UCInput(Guid ActorId) : IRequest<GetPermissions_UCOutput>;
