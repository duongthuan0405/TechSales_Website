namespace TechSalesManagement.Application.Exceptions;

public class ConflictException : BusinessException
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Dictionary<string, List<string>> errors) 
        : base(message, errors)
    {
    }
}
