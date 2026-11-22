using CatalogModule.Application.Messages;
using CatalogModule.Application.UseCases.GetAllBrand;
using CatalogModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogModule.Presentation.Controllers;

[ApiController]
[Route("api/brands")]
[AllowAnonymous]
public sealed class PublicBrandController : ControllerBase
{
    private readonly ISender _sender;

    public PublicBrandController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<BrandResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBrands(CancellationToken cancellationToken)
    {
        GetAllBrand_UCInput input = new GetAllBrand_UCInput();
        GetAllBrand_UCOutput output = await _sender.Send(input, cancellationToken);
        List<BrandResponseDto> brands = new List<BrandResponseDto>();
        foreach (GetAllBrand_UCOutput.BrandItem item in output.Items)
        {
            brands.Add(new BrandResponseDto(item.Id, item.Name));
        }

        ApiSuccessResponse<List<BrandResponseDto>> response = new ApiSuccessResponse<List<BrandResponseDto>>(
            brands, ResponseMessage.ListedBrands);
        return Ok(response);
    }
}
