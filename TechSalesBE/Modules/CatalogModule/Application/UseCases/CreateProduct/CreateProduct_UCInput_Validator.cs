using CatalogModule.Application.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.CreateProduct;

internal sealed class CreateProduct_UCInput_Validator : AbstractValidator<CreateProduct_UCInput>
{
    public CreateProduct_UCInput_Validator()
    {
        RuleFor(input => input.Name).Must(ProductRules.ValidName)
            .WithMessage(BusinessExceptionMessage.ProductNameInvalid);
        RuleFor(input => input.Description).Must(ProductRules.ValidDescription)
            .WithMessage(BusinessExceptionMessage.ProductDescriptionInvalid);
        RuleFor(input => input.Price).Must(ProductRules.ValidPrice)
            .WithMessage(BusinessExceptionMessage.ProductPriceInvalid);
        RuleFor(input => input.BrandId).Must(ProductRules.ValidBrandId)
            .WithMessage(BusinessExceptionMessage.ProductBrandInvalid);
        RuleFor(input => input.CategoryId).Must(ProductRules.ValidCategoryId)
            .WithMessage(BusinessExceptionMessage.ProductCategoryInvalid);
        RuleFor(input => input.Images).Must(images => ProductImageRules.ValidCount(images?.Count ?? 0))
            .WithMessage(BusinessExceptionMessage.ProductImagesInvalid);
        RuleFor(input => input.PrimaryImageIndex)
            .Must((input, index) => ProductImageRules.ValidPrimaryIndex(input.Images?.Count ?? 0, index))
            .WithMessage(BusinessExceptionMessage.PrimaryImageInvalid);
    }
}
