using FluentValidation;
using InventoryModule.Application.Exceptions;
using InventoryModule.Domain.Rules;

namespace InventoryModule.Application.UseCases.UpdateProductUnitStatus;

internal sealed class UpdateProductUnitStatus_UCInput_Validator : AbstractValidator<UpdateProductUnitStatus_UCInput>
{
    public UpdateProductUnitStatus_UCInput_Validator()
    {
        RuleFor(input => input.ProductId)
            .Must(InventoryRules.IsValidProductId)
            .WithMessage(BusinessExceptionMessage.InvalidProductId);

        RuleFor(input => input.SerialNumber)
            .Must(ProductUnitRules.IsValidSerialNumber)
            .WithMessage(BusinessExceptionMessage.InvalidSerialNumber);

        RuleFor(input => input.Status)
            .Must(ProductUnitRules.IsManualStatusTarget)
            .WithMessage(BusinessExceptionMessage.InvalidManualStatus);
    }
}
