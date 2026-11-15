namespace TechSalesManagement.Application.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Dictionary<string, List<string>> errors) 
        : base(message, errors)
    {
    }
}
