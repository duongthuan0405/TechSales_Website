using System;
using MailModule.Domain.Entities;

namespace MailModule.Application.Interfaces;

public interface IVerifyEmaiEmailSender
{
    Task Execute(string toEmail, string token, int expiredInMinutes, CancellationToken cancellationToken = default);
}
