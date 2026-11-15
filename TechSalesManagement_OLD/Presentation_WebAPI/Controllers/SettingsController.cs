using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "Staff,Business Admin,Technical Admin")]
public class SettingsController : ControllerBase
{
    private readonly ISystemSettingService _settingService;

    public SettingsController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<List<SystemSetting>>>> GetAllAsync()
    {
        var settings = await _settingService.GetAllSettingsAsync();
        return Ok(new ApiSuccessResponse<List<SystemSetting>>(settings, "Settings retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UpdateAsync([FromBody] UpdateSettingRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _settingService.UpdateSettingAsync(request.key, request.value, request.description, staffId.Value);
        return Ok(new ApiSuccessResponse<object>(null, $"Setting '{request.key}' updated successfully."));
    }
}

public class UpdateSettingRequestDto
{
    public string key { get; set; } = string.Empty;
    public string value { get; set; } = string.Empty;
    public string? description { get; set; }
}
