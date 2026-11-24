using FluentValidation;
using InventoryModule.Application.Constants;
using InventoryModule.Application.Exceptions;

namespace InventoryModule.Application.UseCases.ImportStock;

internal sealed class ImportStock_UCInput_Validator : AbstractValidator<ImportStock_UCInput>
{
    public ImportStock_UCInput_Validator()
    {
        RuleFor(input => input.Length)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithMessage(BusinessExceptionMessage.ImportFileRequired)
            .LessThanOrEqualTo(StockImportLimits.MaxFileBytes)
            .WithMessage(BusinessExceptionMessage.ImportFileTooLarge)
            .OverridePropertyName("File");

        RuleFor(input => input.FileName)
            .Must(fileName => string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            .When(input => input.Length > 0)
            .WithMessage(BusinessExceptionMessage.InvalidImportFileType)
            .OverridePropertyName("File");
    }
}
