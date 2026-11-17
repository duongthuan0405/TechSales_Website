using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using MediatR;
using ShippingModule.Application.Exceptions;
using ShippingModule.Domain.Exceptions;

namespace ShippingModule.Application.Behaviors;

internal sealed class BusinessRuleExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (DomainException exception)
        {
            switch (exception.Code)
            {
                case DomainExceptionCode.RULE_ADDRESS_NOT_FOUND:
                    throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.AddressNotFound);
                case DomainExceptionCode.RULE_ADDRESS_NOT_OWNED:
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.AddressNotOwned);
                default:
                    throw;
            }
        }
    }
}
