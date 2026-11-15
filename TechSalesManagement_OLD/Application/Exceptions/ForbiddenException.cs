namespace TechSalesManagement.Application.Exceptions;

public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message) : base(message)
    {
    }

    public ForbiddenException(string message, Dictionary<string, List<string>> errors) 
        : base(message, errors)
    {
    }
}
