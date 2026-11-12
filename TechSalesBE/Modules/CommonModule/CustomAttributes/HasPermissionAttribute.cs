using Microsoft.AspNetCore.Authorization;

namespace CommonModule.CustomAttributes;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = $"Permission:{permission}";
    }
}
