using System;
using System.Security.Claims;

namespace TechSalesManagement.Presentation_WebAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var result))
        {
            return result;
        }
        return null;
    }
}
