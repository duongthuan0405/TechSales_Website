using CatalogModule.Application.UseCases.CreateCategory;
using CatalogModule.Application.Messages;
using CatalogModule.Application.UseCases.UpdateCategory;
using CatalogModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogModule.Presentation.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = CategoryAdministrators)]
public sealed class CategoryController : ControllerBase
{
    private const string CategoryAdministrators = "Business Admin,Technical Admin";

    private readonly ISender _sender;

    public CategoryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        CreateCategory_UCInput input = new CreateCategory_UCInput(request.Name);
        CreateCategory_UCOutput output = await _sender.Send(input, cancellationToken);
        CategoryResponseDto data = new CategoryResponseDto(output.Id, output.Name);
        ApiSuccessResponse<CategoryResponseDto> response = new ApiSuccessResponse<CategoryResponseDto>(data);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCategory(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        UpdateCategory_UCInput input = new UpdateCategory_UCInput(id, request.Name);
        UpdateCategory_UCOutput output = await _sender.Send(input, cancellationToken);
        CategoryResponseDto data = new CategoryResponseDto(output.Id, output.Name);
        ApiSuccessResponse<CategoryResponseDto> response = new ApiSuccessResponse<CategoryResponseDto>(
            data,
            ResponseMessage.UpdatedCategory);

        return Ok(response);
    }
}
