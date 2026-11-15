using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

public class TestCreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class TestCreateProductValidator : AbstractValidator<TestCreateProductRequest>
{
    public TestCreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Email).EmailAddress();
    }
}

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpPost("validate")]
    public ActionResult<ApiSuccessResponse<object>> TestValidation([FromBody] TestCreateProductRequest request)
    {
        // If it reaches here, validation has passed
        return Ok(new ApiSuccessResponse<object>(null, "Validation passed successfully"));
    }

    [HttpGet("success")]
    public ActionResult<ApiSuccessResponse<object>> GetSuccess()
    {
        var data = new { Id = 1, Name = "Test Product", Price = 100.00 };
        return Ok(new ApiSuccessResponse<object>(data, "Data retrieved successfully"));
    }

    [HttpGet("not-found")]
    public IActionResult GetNotFound()
    {
        // Simulate product not found -> Middleware will log and handle this
        throw new NotFoundException("Product with ID 999 was not found");
    }

    [HttpGet("conflict")]
    public IActionResult GetConflict()
    {
        // Simulate data conflict -> Middleware will log and handle this
        var errors = new Dictionary<string, List<string>>
        {
            { "Email", new List<string> { "This email is already registered in the system" } }
        };
        
        throw new ConflictException("Invalid data provided", errors);
    }
}
