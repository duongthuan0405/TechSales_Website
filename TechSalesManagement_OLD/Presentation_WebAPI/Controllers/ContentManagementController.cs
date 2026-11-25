using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Common;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/admin/content")]
[Authorize(Roles = "Staff,Business Admin,Technical Admin")]
public class ContentManagementController : ControllerBase
{
    private readonly IContentManagementService _contentService;

    public ContentManagementController(IContentManagementService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("articles")]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<Article>>>> GetArticlesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _contentService.GetPagedArticlesAsync(pageNumber, pageSize);
        var response = new PagedResponseDto<Article>
        {
            items = items,
            totalCount = totalCount,
            pageNumber = pageNumber,
            pageSize = pageSize
        };
        return Ok(new ApiSuccessResponse<PagedResponseDto<Article>>(response, "Articles retrieved successfully."));
    }

    [HttpPost("articles")]
    public async Task<ActionResult<ApiSuccessResponse<Article>>> CreateArticleAsync([FromBody] CreateArticleRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        var article = await _contentService.CreateArticleAsync(request.title, request.content, request.thumbnailUrl, staffId.Value);
        return Ok(new ApiSuccessResponse<Article>(article, "Article created as draft."));
    }

    [HttpPut("articles/{id}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UpdateArticleAsync([FromRoute] Guid id, [FromBody] UpdateArticleRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _contentService.UpdateArticleAsync(id, request.title, request.content, request.thumbnailUrl, staffId.Value);
        return Ok(new ApiSuccessResponse<object>(null, "Article updated successfully."));
    }

    [HttpPatch("articles/{id}/publish")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> PublishArticleAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _contentService.PublishArticleAsync(id, staffId.Value);
        return Ok(new ApiSuccessResponse<object>(null, "Article published successfully."));
    }

    [HttpDelete("articles/{id}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> DeleteArticleAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _contentService.DeleteArticleAsync(id, staffId.Value);
        return Ok(new ApiSuccessResponse<object>(null, "Article deleted successfully."));
    }
}

public class CreateArticleRequestDto
{
    public string title { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string? thumbnailUrl { get; set; }
}

public class UpdateArticleRequestDto : CreateArticleRequestDto { }
