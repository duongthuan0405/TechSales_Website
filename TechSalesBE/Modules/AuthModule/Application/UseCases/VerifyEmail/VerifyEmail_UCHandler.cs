using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.VerifyEmail;
internal sealed class VerifyEmail_UCHandler : IRequestHandler<VerifyEmail_UCInput, VerifyEmail_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IAuthOtpManager _tokens;
    private readonly IExecuteAtomically _transaction;

    public VerifyEmail_UCHandler(IUserRepository users, IAuthOtpManager tokens, IExecuteAtomically transaction)
    {
        _users = users;
        _tokens = tokens;
        _transaction = transaction;
    }

    public async Task<VerifyEmail_UCOutput> Handle(VerifyEmail_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            var user = await _users.GetUserByEmailAsync(input.Email, ct, forUpdate: true) ?? throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "Verification code is invalid or expired.");
            await _tokens.ConsumeOtpAsync(user.Id, OtpType.EMAIL_VERIFICATION, input.Token, ct);
            user.VerifyEmail(DateTimeOffset.UtcNow);
            await _users.UpdateAsync(user, ct);
            return new VerifyEmail_UCOutput("Email verified successfully.");
        }, ct);
    }
}
