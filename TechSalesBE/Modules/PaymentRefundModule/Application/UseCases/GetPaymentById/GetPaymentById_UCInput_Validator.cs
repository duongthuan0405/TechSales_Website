using FluentValidation;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.GetPaymentById;

internal sealed class GetPaymentById_UCInput_Validator : AbstractValidator<GetPaymentById_UCInput>
{
    public GetPaymentById_UCInput_Validator()
    {
        RuleFor(request => request.CustomerId).NotEmpty().WithMessage(BusinessExceptionMessage.CustomerIdInvalid);
        RuleFor(request => request.Id).NotEmpty().WithMessage(BusinessExceptionMessage.PaymentIdInvalid);
    }
}