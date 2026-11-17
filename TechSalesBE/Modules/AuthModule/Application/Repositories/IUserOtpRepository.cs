using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
namespace AuthModule.Application.Repositories;
internal interface IUserOtpRepository
{
    Task<UserOtp?> GetByUserIdAndTypeAsync(Guid userId, OtpType type, CancellationToken cancellationToken = default);
    Task UpsertAsync(UserOtp token, CancellationToken cancellationToken);
    Task InvalidateAsync(Guid userId, OtpType type, DateTimeOffset now, CancellationToken cancellationToken);
}
