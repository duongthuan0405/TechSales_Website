using FluentValidation;
using InventoryModule.Application.Exceptions;
using InventoryModule.Domain.Rules;

namespace InventoryModule.Application.UseCases.ReceiveStock;

internal sealed class ReceiveStock_UCInput_Validator : AbstractValidator<ReceiveStock_UCInput>
{
    public ReceiveStock_UCInput_Validator()
    {
        RuleFor(input => input.ProductId)
            .Must(InventoryRules.IsValidProductId)
            .WithMessage(BusinessExceptionMessage.InvalidProductId);

        RuleFor(input => input.SerialNumbers)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.SerialNumbersRequired)
            .Must(ProductUnitRules.HasDistinctSerialNumbers)
            .WithMessage(BusinessExceptionMessage.DuplicateSerialNumbers);

        RuleForEach(input => input.SerialNumbers)
            .Must(ProductUnitRules.IsValidSerialNumber)
            .WithMessage(BusinessExceptionMessage.InvalidSerialNumber);
    }
}
