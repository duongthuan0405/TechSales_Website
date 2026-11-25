namespace TechSalesManagement.Application.Exceptions;

public class BadRequestException : BusinessException
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Dictionary<string, List<string>> errors) 
        : base(message, errors)
    {
    }
}
