using CatalogModule.Application.Messages;
using CatalogModule.Application.UseCases.GetAllCategory;
using CatalogModule.Application.UseCases.GetCategoryById;
using CatalogModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogModule.Presentation.Controllers;

[ApiController]
[Route("api/categories")]
[AllowAnonymous]
public sealed class PublicCategoryController : ControllerBase
{
    private readonly ISender _sender;

    public PublicCategoryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<CategoryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        GetAllCategory_UCInput input = new GetAllCategory_UCInput();
        GetAllCategory_UCOutput output = await _sender.Send(input, cancellationToken);
        List<CategoryResponseDto> categories = new List<CategoryResponseDto>();

        foreach (GetAllCategory_UCOutput.CategoryItem item in output.Items)
        {
            categories.Add(new CategoryResponseDto(item.Id, item.Name));
        }

        ApiSuccessResponse<List<CategoryResponseDto>> response = new ApiSuccessResponse<List<CategoryResponseDto>>(
            categories,
            ResponseMessage.ListedCategories);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        GetCategoryById_UCInput input = new GetCategoryById_UCInput(id);
        GetCategoryById_UCOutput output = await _sender.Send(input, cancellationToken);
        CategoryResponseDto category = new CategoryResponseDto(output.Id, output.Name);
        ApiSuccessResponse<CategoryResponseDto> response = new ApiSuccessResponse<CategoryResponseDto>(
            category,
            ResponseMessage.RetrievedCategory);
        return Ok(response);
    }
}
