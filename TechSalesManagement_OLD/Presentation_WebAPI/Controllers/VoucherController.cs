using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Common;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/vouchers")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherManagementService _voucherService;

    public VoucherController(IVoucherManagementService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<Voucher>>>> GetAvailableVouchersAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        // Reuse the same service method but filter/logic could be added if needed
        var (items, totalCount) = await _voucherService.GetAllVouchersAsync(pageNumber, pageSize);
        
        // Filter only active and not expired for customers
        var availableItems = items.Where(v => v.isActive && !v.IsExpired() && v.usedCount < v.maxUsage).ToList();

        var response = new PagedResponseDto<Voucher>
        {
            items = availableItems,
            totalCount = availableItems.Count,
            pageNumber = pageNumber,
            pageSize = pageSize
        };
        return Ok(new ApiSuccessResponse<PagedResponseDto<Voucher>>(response, "Vouchers retrieved successfully."));
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ApiSuccessResponse<Voucher>>> ValidateVoucherAsync([FromBody] ValidateVoucherRequestDto request)
    {
        var voucher = await _voucherService.ValidateVoucherAsync(request.code, request.orderAmount);
        return Ok(new ApiSuccessResponse<Voucher>(voucher, "Voucher is valid."));
    }
}

public class ValidateVoucherRequestDto
{
    public string code { get; set; } = string.Empty;
    public decimal orderAmount { get; set; }
}
