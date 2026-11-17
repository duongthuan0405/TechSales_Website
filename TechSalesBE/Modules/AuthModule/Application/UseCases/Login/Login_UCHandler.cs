using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.Login;
internal sealed class Login_UCHandler : IRequestHandler<Login_UCInput, Login_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IAccessTokenService _jwt;
    private readonly IExecuteAtomically _transaction;

    public Login_UCHandler(IUserRepository users, IPasswordHasher hasher, IAccessTokenService jwt, IExecuteAtomically transaction)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
        _transaction = transaction;
    }

    public async Task<Login_UCOutput> Handle(Login_UCInput input, CancellationToken ct)
    {
        // Commit failed-attempt counters before returning an authentication error.
        (Login_UCOutput? Output, bool Blocked) result = await _transaction.ExecuteAtomicallyAsync<(Login_UCOutput? Output, bool Blocked)>(async () =>
        {
            User? user = await _users.GetUserByEmailAsync(input.Email, ct, forUpdate: true);
            if (user == null)
            {
                _hasher.VerifyPassword(input.Password, null);
                return (null, false);
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            user.ReleaseExpiredLock(now);
            if (user.Status == UserStatus.BLOCKED)
            {
                return (null, true);
            }

            if (user.Status != UserStatus.ACTIVE)
            {
                return (null, false);
            }

            if (!_hasher.VerifyPassword(input.Password, user.Password))
            {
                user.RecordFailedLogin(now);
                await _users.UpdateAsync(user, ct);

                return (null, user.Status == UserStatus.BLOCKED);
            }

            user.RecordSuccessfulLogin(now);
            await _users.UpdateAsync(user, ct);
            IssuedAccessToken issued = _jwt.Issue(user);
            List<string> roleNames = new List<string>();
            foreach (Role role in user.Roles)
            {
                roleNames.Add(role.Name);
            }

            Login_UCOutput output = new Login_UCOutput(issued.Token, user.Email, roleNames, issued.ExpiresAt);
            return (output, false);
        }, ct);
        if (result.Output != null)
        {
            return result.Output;
        }

        string errorCode = BusinessExceptionCode.UNAUTHORIZED;
        string message = "Email or password is incorrect, or the account is not verified.";
        if (result.Blocked)
        {
            errorCode = BusinessExceptionCode.FORBIDDEN;
            message = "Account is locked.";
        }

        throw new BusinessException(errorCode, message);
    }
}
