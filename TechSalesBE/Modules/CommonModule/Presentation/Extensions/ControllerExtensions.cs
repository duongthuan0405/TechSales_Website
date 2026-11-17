using System.Security.Claims;
using CommonModule.BusinessExceptions;
using Microsoft.AspNetCore.Mvc;

namespace CommonModule.Presentation.Extensions;

public static class ControllerExtensions
{
    public static Guid GetActorId(this ControllerBase controller)
    {
        string? subject = controller.User.FindFirstValue("sub");
        Guid actorId;
        bool isValidId = Guid.TryParse(subject, out actorId);

        if (!isValidId)
        {
            throw new BusinessException(BusinessExceptionCode.UNAUTHORIZED, "Invalid identity.");
        }

        return actorId;
    }

}
