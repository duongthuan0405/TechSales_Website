using CommonModule.Application.Exceptions;
using System.Security.Claims;
using CommonModule.BusinessExceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CommonModule.Presentation.Extensions;

public static class ControllerExtensions
{
    public static Guid GetActorId(this ControllerBase controller)
    {
        string? subject = controller.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        Guid actorId;
        bool isValidId = Guid.TryParse(subject, out actorId);

        if (!isValidId)
        {
            throw new BusinessException(BusinessExceptionCode.UNAUTHORIZED, BusinessExceptionMessage.IdentityInvalid);
        }

        return actorId;
    }

}
