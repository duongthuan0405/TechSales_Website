using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Technical Admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<AuditLog>>>> GetLogsAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] Guid? userId = null)
    {
        var (items, totalCount) = await _auditService.GetSystemLogsAsync(pageNumber, pageSize, userId);
        var response = new PagedResponseDto<AuditLog>
        {
            items = items,
            totalCount = totalCount,
            pageNumber = pageNumber,
            pageSize = pageSize
        };
        return Ok(new ApiSuccessResponse<PagedResponseDto<AuditLog>>(response, "Audit logs retrieved successfully."));
    }
}
