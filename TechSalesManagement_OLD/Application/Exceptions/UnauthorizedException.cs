namespace TechSalesManagement.Application.Exceptions;

public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Dictionary<string, List<string>> errors) 
        : base(message, errors)
    {
    }
}
