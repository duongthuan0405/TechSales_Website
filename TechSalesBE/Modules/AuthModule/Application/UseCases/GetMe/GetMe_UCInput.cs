using MediatR;
namespace AuthModule.Application.UseCases.GetMe;
internal sealed record GetMe_UCInput(Guid ActorId) : IRequest<GetMe_UCOutput>;
