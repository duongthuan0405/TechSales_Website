using CatalogModule.Application.Messages;
using CatalogModule.Application.Services;
using CatalogModule.Application.UseCases.CreateProduct;
using CatalogModule.Application.UseCases.UpdateProduct;
using CatalogModule.Application.UseCases.UpdateProductImages;
using CatalogModule.Presentation.Authorization;
using CatalogModule.Presentation.DTOs;
using CatalogModule.Presentation.Configuration;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogModule.Presentation.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = RoleAccess.Administrators)]
public sealed class ProductManagementController : ControllerBase
{
    private readonly ISender _sender;

    public ProductManagementController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateProduct_UCOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        UpdateProduct_UCInput input = new UpdateProduct_UCInput(id, request.Name, request.Description,
            request.Price, request.BrandId, request.CategoryId, request.Status);
        UpdateProduct_UCOutput output = await _sender.Send(input, cancellationToken);
        return Ok(new ApiSuccessResponse<UpdateProduct_UCOutput>(output, ResponseMessage.ProductUpdated));
    }

    [HttpPut("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProductUploadLimits.MaximumRequestSizeBytes)]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateProductImages_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProductImages(Guid id, [FromForm] UpdateProductImagesRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new UpdateProductImagesRequest();
        List<ProductImageUpload> images = new List<ProductImageUpload>();
        try
        {
            foreach (IFormFile file in request.Images)
            {
                images.Add(new ProductImageUpload(file.OpenReadStream(), file.ContentType, file.Length));
            }
            UpdateProductImages_UCOutput output = await _sender.Send(new UpdateProductImages_UCInput(
                id, request.RetainedImageIds, images, request.PrimaryImageIndex), cancellationToken);
            return Ok(new ApiSuccessResponse<UpdateProductImages_UCOutput>(output, ResponseMessage.ProductUpdated));
        }
        finally
        {
            foreach (ProductImageUpload image in images)
            {
                await image.Content.DisposeAsync();
            }
        }
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiSuccessResponse<CreateProduct_UCOutput>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        CreateProduct_UCInput input = new CreateProduct_UCInput(request.Name, request.Description,
            request.Price, request.BrandId, request.CategoryId);
        CreateProduct_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<CreateProduct_UCOutput> response =
            new ApiSuccessResponse<CreateProduct_UCOutput>(output, ResponseMessage.ProductCreated);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("with-images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProductUploadLimits.MaximumRequestSizeBytes)]
    [ProducesResponseType(typeof(ApiSuccessResponse<CreateProduct_UCOutput>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProductWithImages([FromForm] CreateProductFormRequest request,
        CancellationToken cancellationToken)
    {
        List<ProductImageUpload> images = new List<ProductImageUpload>();
        try
        {
            foreach (IFormFile file in request.Images)
            {
                images.Add(new ProductImageUpload(file.OpenReadStream(), file.ContentType, file.Length));
            }

            CreateProduct_UCInput input = new CreateProduct_UCInput(request.Name, request.Description,
                request.Price, request.BrandId, request.CategoryId, images, request.PrimaryImageIndex);
            CreateProduct_UCOutput output = await _sender.Send(input, cancellationToken);
            ApiSuccessResponse<CreateProduct_UCOutput> response =
                new ApiSuccessResponse<CreateProduct_UCOutput>(output, ResponseMessage.ProductCreated);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        finally
        {
            foreach (ProductImageUpload image in images)
            {
                await image.Content.DisposeAsync();
            }
        }
    }
}
