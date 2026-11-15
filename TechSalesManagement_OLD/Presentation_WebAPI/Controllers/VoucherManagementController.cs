using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Common;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/admin/vouchers")]
[Authorize(Roles = "Staff,Business Admin,Technical Admin")]
public class VoucherManagementController : ControllerBase
{
    private readonly IVoucherManagementService _voucherService;

    public VoucherManagementController(IVoucherManagementService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<Voucher>>>> GetAllAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _voucherService.GetAllVouchersAsync(pageNumber, pageSize);
        var response = new PagedResponseDto<Voucher>
        {
            items = items,
            totalCount = totalCount,
            pageNumber = pageNumber,
            pageSize = pageSize
        };
        return Ok(new ApiSuccessResponse<PagedResponseDto<Voucher>>(response, "Vouchers retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiSuccessResponse<Voucher>>> CreateAsync([FromBody] CreateVoucherRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        var voucher = await _voucherService.CreateVoucherAsync(
            request.code, request.type, request.value, request.maxUsage, request.minOrderAmount, request.startDate, request.endDate, staffId.Value);

        return Ok(new ApiSuccessResponse<Voucher>(voucher, "Voucher created successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> DeleteAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _voucherService.DeleteVoucherAsync(id, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, "Voucher deleted successfully."));
    }
}

public class CreateVoucherRequestDto
{
    public string code { get; set; } = string.Empty;
    public VoucherType type { get; set; }
    public decimal value { get; set; }
    public int maxUsage { get; set; }
    public decimal minOrderAmount { get; set; }
    public DateTimeOffset? startDate { get; set; }
    public DateTimeOffset? endDate { get; set; }
}
