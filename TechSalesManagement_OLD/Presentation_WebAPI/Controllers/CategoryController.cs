using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<List<Category>>>> GetAllAsync()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(new ApiSuccessResponse<List<Category>>(categories, "Categories retrieved successfully."));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiSuccessResponse<object>>> CreateAsync([FromBody] CreateCategoryRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _categoryService.CreateCategoryAsync(request.name, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG72));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> DeleteAsync([FromRoute] Guid id, [FromQuery] Guid replacementCategoryId)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _categoryService.DeleteCategoryAsync(id, replacementCategoryId, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG74));
    }
}
