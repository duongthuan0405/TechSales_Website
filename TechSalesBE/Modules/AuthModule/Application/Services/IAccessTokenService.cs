using AuthModule.Domain.Entities;
namespace AuthModule.Application.Services;
internal interface IAccessTokenService
{
    IssuedAccessToken Issue(User user);
}
