using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Ports;
using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogModule.Application.UseCases.UpdateProductImages;

internal sealed class UpdateProductImages_UCHandler : IRequestHandler<UpdateProductImages_UCInput, UpdateProductImages_UCOutput>
{
    private readonly IProductRepository _products;
    private readonly IProductUnitOfWork _unitOfWork;
    private readonly IProductImageStorage _storage;
    private readonly ILogger<UpdateProductImages_UCHandler> _logger;

    public UpdateProductImages_UCHandler(IProductRepository products, IProductUnitOfWork unitOfWork,
        IProductImageStorage storage, ILogger<UpdateProductImages_UCHandler> logger)
    {
        _products = products;
        _unitOfWork = unitOfWork;
        _storage = storage;
        _logger = logger;
    }

    public async Task<UpdateProductImages_UCOutput> Handle(UpdateProductImages_UCInput input, CancellationToken cancellationToken)
    {
        Product? product = await _products.GetByIdAsync(input.Id, cancellationToken);
        if (product == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductNotFound);
        }
        List<ProductImage> oldImages = product.Images;
        // Validate ownership and duplicate IDs before writing any files.
        product.UpdateImages(input.RetainedImageIds, new List<string>(), null, DateTimeOffset.UtcNow);
        List<string> newUrls = new List<string>();
        try
        {
            foreach (ProductImageUpload image in input.Images)
            {
                newUrls.Add(await _storage.SaveAsync(image.Content, image.ContentType, image.Length, cancellationToken));
            }
            product.UpdateImages(input.RetainedImageIds, newUrls, input.PrimaryImageIndex, DateTimeOffset.UtcNow);
            await _products.UpdateImagesAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await DeleteFilesAsync(newUrls);
            throw;
        }
        await DeleteFilesAsync(oldImages.Where(image => !input.RetainedImageIds.Contains(image.Id))
            .Select(image => image.ImageUrl).ToList());
        return new UpdateProductImages_UCOutput(product.Id, product.Images.Select(image =>
            new UpdatedProductImage(image.Id, image.ImageUrl, image.IsPrimary)).ToList());
    }

    private async Task DeleteFilesAsync(List<string> urls)
    {
        foreach (string url in urls)
        {
            try
            {
                await _storage.DeleteAsync(url, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Cannot clean up product image {ImageUrl}.", url);
            }
        }
    }
}
