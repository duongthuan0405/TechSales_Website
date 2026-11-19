using CommonModule.BusinessExceptions;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.Services;

internal static class PaymentInputParser
{
    public static PaymentType RequiredType(string? value)
    {
        if (!PaymentRules.ValidType(value))
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentTypeInvalid);
        }

        return Enum.Parse<PaymentType>(value!.Trim(), ignoreCase: true);
    }

    public static PaymentType? OptionalType(string? value)
    {
        if (value == null)
        {
            return null;
        }

        return RequiredType(value);
    }

    public static TransactionStatus? OptionalStatus(string? value)
    {
        if (value == null)
        {
            return null;
        }

        if (!PaymentRules.ValidStatus(value))
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentStatusInvalid);
        }

        return Enum.Parse<TransactionStatus>(value.Trim(), ignoreCase: true);
    }
}
