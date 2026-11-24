using CatalogModule.Application.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.UpdateProduct;

internal sealed class UpdateProduct_UCInput_Validator : AbstractValidator<UpdateProduct_UCInput>
{
    public UpdateProduct_UCInput_Validator()
    {
        RuleFor(input => input.Id).NotEmpty().WithMessage(BusinessExceptionMessage.ProductNotFound);
        RuleFor(input => input.Name).Must(ProductRules.ValidName).WithMessage(BusinessExceptionMessage.ProductNameInvalid);
        RuleFor(input => input.Description).Must(ProductRules.ValidDescription).WithMessage(BusinessExceptionMessage.ProductDescriptionInvalid);
        RuleFor(input => input.Price).Must(ProductRules.ValidPrice).WithMessage(BusinessExceptionMessage.ProductPriceInvalid);
        RuleFor(input => input.BrandId).Must(ProductRules.ValidBrandId).WithMessage(BusinessExceptionMessage.ProductBrandInvalid);
        RuleFor(input => input.CategoryId).Must(ProductRules.ValidCategoryId).WithMessage(BusinessExceptionMessage.ProductCategoryInvalid);
        RuleFor(input => input.Status).Must(ProductRules.ValidStatus).WithMessage(BusinessExceptionMessage.ProductStatusInvalid);
    }
}
