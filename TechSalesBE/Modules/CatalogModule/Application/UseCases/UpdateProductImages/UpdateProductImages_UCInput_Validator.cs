using CatalogModule.Application.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.UpdateProductImages;

internal sealed class UpdateProductImages_UCInput_Validator : AbstractValidator<UpdateProductImages_UCInput>
{
    public UpdateProductImages_UCInput_Validator()
    {
        RuleFor(input => input.Id).NotEmpty().WithMessage(BusinessExceptionMessage.ProductNotFound);
        RuleFor(input => input).Must(input => ProductImageRules.ValidCount(input.RetainedImageIds.Count + input.Images.Count))
            .WithMessage(BusinessExceptionMessage.ProductImagesInvalid);
        RuleFor(input => input).Must(input => ProductImageRules.ValidPrimaryIndex(
            input.RetainedImageIds.Count + input.Images.Count, input.PrimaryImageIndex))
            .WithMessage(BusinessExceptionMessage.PrimaryImageInvalid);
    }
}
