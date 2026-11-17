using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.Register;
internal sealed class Register_UCHandler : IRequestHandler<Register_UCInput, Register_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _hasher;
    private readonly IAuthOtpManager _tokens;
    private readonly IAuthEmailAdapter _mail;
    private readonly IExecuteAtomically _transaction;

    public Register_UCHandler(IUserRepository users, IRoleRepository roles, IPasswordHasher hasher, IAuthOtpManager tokens, IAuthEmailAdapter mail, IExecuteAtomically transaction)
    {
        _users = users;
        _roles = roles;
        _hasher = hasher;
        _tokens = tokens;
        _mail = mail;
        _transaction = transaction;
    }

    public async Task<Register_UCOutput> Handle(Register_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            var user = await _users.GetUserByEmailAsync(input.Email, ct, forUpdate: true);
            if (user is not null && user.Status != UserStatus.PENDING)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, "Email is already registered.");
            }

            if (user is null)
            {
                user = User.Create(input.Email, _hasher.HashPassword(input.Password));
                var customer = await _roles.GetByNameAsync(UserRoleName.CUSTOMER, ct) ?? throw new InvalidOperationException("Apply the Auth role migration first.");
                user.Roles.Add(customer);
                await _users.AddAsync(user, ct);
            }
            else
            {
                user.ChangePassword(_hasher.HashPassword(input.Password), DateTimeOffset.UtcNow);
                await _users.UpdateAsync(user, ct);
            }

            var code = await _tokens.CreateOtpAsync(user.Id, OtpType.EMAIL_VERIFICATION, ct);
            await _mail.SendVerificationAsync(user.Email, code, ct);
            return new Register_UCOutput("Registration received. Check your email to verify your account.");
        }, ct);
    }
}
