using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Application.Ports;
using CatalogModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogModule.Application.UseCases.CreateProduct;

internal sealed class CreateProduct_UCHandler : IRequestHandler<CreateProduct_UCInput, CreateProduct_UCOutput>
{
    private readonly IProductRepository _products;
    private readonly IProductReferenceData _references;
    private readonly IProductUnitOfWork _unitOfWork;
    private readonly IProductImageStorage _imageStorage;
    private readonly ILogger<CreateProduct_UCHandler> _logger;

    public CreateProduct_UCHandler(IProductRepository products, IProductReferenceData references,
        IProductUnitOfWork unitOfWork,
        IProductImageStorage imageStorage, ILogger<CreateProduct_UCHandler> logger)
    {
        _products = products;
        _references = references;
        _unitOfWork = unitOfWork;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<CreateProduct_UCOutput> Handle(CreateProduct_UCInput input,
        CancellationToken cancellationToken)
    {
        bool brandExists = (await _references.GetBrandsAsync(cancellationToken))
            .Any(brand => brand.Id == input.BrandId);
        if (!brandExists)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.BrandNotFound);
        }

        bool categoryExists = (await _references.GetCategoriesAsync(cancellationToken))
            .Any(category => category.Id == input.CategoryId);
        if (!categoryExists)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.CategoryNotFound);
        }

        List<string> imageUrls = new List<string>();
        try
        {
            if (input.Images != null)
            {
                foreach (ProductImageUpload image in input.Images)
                {
                    imageUrls.Add(await _imageStorage.SaveAsync(image.Content, image.ContentType,
                        image.Length, cancellationToken));
                }
            }

            Product product = Product.Create(input.Name, input.Description, input.Price,
                input.BrandId, input.CategoryId, DateTimeOffset.UtcNow, imageUrls, input.PrimaryImageIndex);
            await _products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            List<CreateProductImage_UCOutput> images = product.Images.Select(image =>
                new CreateProductImage_UCOutput(image.Id, image.ImageUrl, image.IsPrimary)).ToList();
            return new CreateProduct_UCOutput(product.Id, product.Name, product.Description, product.Price,
                product.Status.ToString(), product.BrandId, product.CategoryId, product.CreatedAt, product.UpdatedAt, images);
        }
        catch
        {
            foreach (string imageUrl in imageUrls)
            {
                try
                {
                    await _imageStorage.DeleteAsync(imageUrl, CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(cleanupException, "Cannot clean up product image {ImageUrl}.", imageUrl);
                }
            }

            throw;
        }
    }
}
