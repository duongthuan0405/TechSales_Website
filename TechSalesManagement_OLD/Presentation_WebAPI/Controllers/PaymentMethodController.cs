using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/payment-method")]
public class PaymentMethodController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<List<PaymentMethod>>>> GetAll()
    {
        var methods = await _paymentMethodService.GetAllPaymentMethodsAsync();
        return Ok(new ApiSuccessResponse<List<PaymentMethod>>(methods));
    }
}
