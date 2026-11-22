using ProductModule.Application.Exceptions;
using ProductModule.Application.Ports;
using ProductModule.Application.Repositories;
using ProductModule.Application.Services;
using ProductModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using MediatR;

namespace ProductModule.Application.UseCases.UpdateProduct;

internal sealed class UpdateProduct_UCHandler : IRequestHandler<UpdateProduct_UCInput, UpdateProduct_UCOutput>
{
    private readonly IProductRepository _products;
    private readonly IProductReferenceData _references;
    private readonly IProductUnitOfWork _unitOfWork;

    public UpdateProduct_UCHandler(IProductRepository products, IProductReferenceData references,
        IProductUnitOfWork unitOfWork)
    {
        _products = products;
        _references = references;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProduct_UCOutput> Handle(UpdateProduct_UCInput input, CancellationToken cancellationToken)
    {
        Product? product = await _products.GetByIdAsync(input.Id, cancellationToken);
        if (product == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductNotFound);
        }
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

        product.Update(input.Name, input.Description, input.Price, input.BrandId, input.CategoryId,
            input.Status, DateTimeOffset.UtcNow);
        await _products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        List<UpdateProductImage_UCOutput> images = product.Images.Select(image =>
            new UpdateProductImage_UCOutput(image.Id, image.ImageUrl, image.IsPrimary)).ToList();
        return new UpdateProduct_UCOutput(product.Id, product.Name, product.Description, product.Price,
            product.Status.ToString(), product.BrandId, product.CategoryId, product.CreatedAt, product.UpdatedAt, images);
    }
}
