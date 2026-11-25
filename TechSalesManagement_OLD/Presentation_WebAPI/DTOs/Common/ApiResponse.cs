using System.Collections.Generic;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.Common;

public abstract class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiSuccessResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public ApiSuccessResponse(T? data, string message = "Operation successful")
    {
        Success = true;
        Message = message;
        Data = data;
    }
}

public class ApiErrorResponse : ApiResponse
{
    public Dictionary<string, List<string>>? Data { get; set; }

    public ApiErrorResponse(string message, Dictionary<string, List<string>>? errors = null)
    {
        Success = true;
        Success = false;
        Message = message;
        Data = errors;
    }
}
