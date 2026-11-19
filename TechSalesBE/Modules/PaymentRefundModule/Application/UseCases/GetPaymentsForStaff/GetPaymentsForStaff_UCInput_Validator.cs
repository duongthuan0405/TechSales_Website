using FluentValidation;
using PaymentRefundModule.Domain.Rules;
using PaymentRefundModule.Application.Constants;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.GetPaymentsForStaff;

internal sealed class GetPaymentsForStaff_UCInput_Validator : AbstractValidator<GetPaymentsForStaff_UCInput>
{
    public GetPaymentsForStaff_UCInput_Validator()
    {
        RuleFor(request => request.PageNumber).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(BusinessExceptionMessage.PaymentPaginationInvalid)
            .Must((request, pageNumber) => request.PageSize <= 0 || ((long)pageNumber - 1) * request.PageSize <= int.MaxValue)
            .WithMessage(BusinessExceptionMessage.PaymentPaginationInvalid);
        RuleFor(request => request.PageSize).InclusiveBetween(1, PaginationLimits.MaximumPageSize)
            .WithMessage(BusinessExceptionMessage.PaymentPaginationInvalid);
        RuleFor(request => request.Type).Must(PaymentRules.ValidType).When(request => request.Type != null)
            .WithMessage(BusinessExceptionMessage.PaymentTypeInvalid);
        RuleFor(request => request.Status).Must(PaymentRules.ValidStatus).When(request => request.Status != null)
            .WithMessage(BusinessExceptionMessage.PaymentStatusInvalid);
        RuleFor(request => request.RefId).Must(refId => refId != Guid.Empty).When(request => request.RefId.HasValue)
            .WithMessage(BusinessExceptionMessage.PaymentReferenceInvalid);
    }
}
