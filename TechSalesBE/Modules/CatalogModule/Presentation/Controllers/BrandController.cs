using CatalogModule.Application.UseCases.CreateBrand;
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
}
