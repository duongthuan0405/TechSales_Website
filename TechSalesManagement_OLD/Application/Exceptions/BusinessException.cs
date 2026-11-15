using System.Net;

namespace TechSalesManagement.Application.Exceptions;

public class BusinessException : Exception
{
    public Dictionary<string, List<string>>? Errors { get; }

    public BusinessException(string message) 
        : base(message)
    {
    }

    public BusinessException(string message, Dictionary<string, List<string>> errors) 
        : base(message)
    {
        Errors = errors;
    }
}
