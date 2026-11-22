using ProductModule.Application.Messages;
using ProductModule.Application.UseCases.BrowseProducts;
using ProductModule.Presentation.Authorization;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductModule.Presentation.Controllers;

[ApiController]
public sealed class ProductQueryController : ControllerBase
{
    private readonly ISender _sender;

    public ProductQueryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("api/products")]
    [AllowAnonymous]
    public async Task<IActionResult> Browse(int pageNumber = 1, int pageSize = 20, string? keyword = null,
        Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        BrowseProducts_UCOutput output = await _sender.Send(new BrowseProducts_UCInput(false,
            pageNumber, pageSize, keyword, categoryId), cancellationToken);
        return Ok(new ApiSuccessResponse<BrowseProducts_UCOutput>(output, ResponseMessage.ProductsListed));
    }

    [HttpGet("api/products/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        BrowseProducts_UCOutput output = await _sender.Send(new BrowseProducts_UCInput(false,
            ProductId: id), cancellationToken);
        return Ok(new ApiSuccessResponse<BrowseProducts_UCOutput>(output, ResponseMessage.ProductsListed));
    }

    [HttpGet("api/admin/products")]
    [Authorize(Roles = RoleAccess.Readers)]
    public async Task<IActionResult> BrowseForManagement(int pageNumber = 1, int pageSize = 20,
        string? keyword = null, Guid? categoryId = null, string? status = null,
        CancellationToken cancellationToken = default)
    {
        BrowseProducts_UCOutput output = await _sender.Send(new BrowseProducts_UCInput(true,
            pageNumber, pageSize, keyword, categoryId, status), cancellationToken);
        return Ok(new ApiSuccessResponse<BrowseProducts_UCOutput>(output, ResponseMessage.ProductsListed));
    }

    [HttpGet("api/admin/products/{id:guid}")]
    [Authorize(Roles = RoleAccess.Readers)]
    public async Task<IActionResult> GetForManagement(Guid id, CancellationToken cancellationToken)
    {
        BrowseProducts_UCOutput output = await _sender.Send(new BrowseProducts_UCInput(true,
            ProductId: id), cancellationToken);
        return Ok(new ApiSuccessResponse<BrowseProducts_UCOutput>(output, ResponseMessage.ProductsListed));
    }
}
