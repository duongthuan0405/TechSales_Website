using AuthModule.Application.Messages;
using AuthModule.Domain.Rules;
using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.ForgotPassword;
internal sealed class ForgotPassword_UCHandler : IRequestHandler<ForgotPassword_UCInput, ForgotPassword_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IAuthOtpManager _tokens;
    private readonly IAuthEmailAdapter _mail;
    private readonly IExecuteAtomically _transaction;

    public ForgotPassword_UCHandler(IUserRepository users, IAuthOtpManager tokens, IAuthEmailAdapter mail, IExecuteAtomically transaction)
    {
        _users = users;
        _tokens = tokens;
        _mail = mail;
        _transaction = transaction;
    }

    public async Task<ForgotPassword_UCOutput> Handle(ForgotPassword_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            var user = await _users.GetUserByEmailAsync(input.Email, ct, forUpdate: true);
            if (AuthWorkflowRules.CanResetPassword(user))
            {
                var code = await _tokens.CreateOtpAsync(user.Id, OtpType.RESET_PASSWORD, ct);
                await _mail.SendPasswordResetAsync(user.Email, code, ct);
            }

            return new ForgotPassword_UCOutput(ResponseMessage.SuccessResetRequested);
        }, ct);
    }
}
