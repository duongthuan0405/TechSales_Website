using CatalogModule.Application.UseCases.BrowseBrandsForManagement;
using CatalogModule.Application.Constants;
using CatalogModule.Application.UseCases.CreateBrand;
using CatalogModule.Application.Messages;
using CatalogModule.Application.UseCases.UpdateBrand;
using CatalogModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogModule.Presentation.Controllers;

[ApiController]
[Route("api/admin/brands")]
[Authorize(Roles = BrandAdministrators)]
public sealed class BrandController : ControllerBase
{
    private const string BrandAdministrators = "Business Admin,Technical Admin";

    private readonly ISender _sender;

    public BrandController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<BrowseBrandsForManagement_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Browse(int pageNumber = 1, int pageSize = CatalogPagination.DefaultPageSize,
        string? keyword = null, CancellationToken cancellationToken = default)
    {
        BrowseBrandsForManagement_UCOutput output = await _sender.Send(
            new BrowseBrandsForManagement_UCInput(pageNumber, pageSize, keyword), cancellationToken);
        return Ok(new ApiSuccessResponse<BrowseBrandsForManagement_UCOutput>(output, ResponseMessage.ListedBrands));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<BrandResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequestDto request, CancellationToken cancellationToken)
    {
        CreateBrand_UCInput input = new CreateBrand_UCInput(request.Name);
        CreateBrand_UCOutput output = await _sender.Send(input, cancellationToken);
        BrandResponseDto data = new BrandResponseDto(output.Id, output.Name);
        ApiSuccessResponse<BrandResponseDto> response = new ApiSuccessResponse<BrandResponseDto>(data);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<BrandResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandRequestDto request,
        CancellationToken cancellationToken)
    {
        UpdateBrand_UCInput input = new UpdateBrand_UCInput(id, request.Name);
        UpdateBrand_UCOutput output = await _sender.Send(input, cancellationToken);
        BrandResponseDto data = new BrandResponseDto(output.Id, output.Name);
        ApiSuccessResponse<BrandResponseDto> response = new ApiSuccessResponse<BrandResponseDto>(
            data, ResponseMessage.UpdatedBrand);

        return Ok(response);
    }
}
