using CommonModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CommonModule.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        foreach (IValidator<TRequest> validator in _validators)
        {
            ValidationResult result = await validator.ValidateAsync(request, cancellationToken);
            foreach (ValidationFailure failure in result.Errors)
            {
                if (!errors.TryGetValue(failure.PropertyName, out List<string>? messages))
                {
                    messages = new List<string>();
                    errors.Add(failure.PropertyName, messages);
                }

                messages.Add(failure.ErrorMessage);
            }
        }

        if (errors.Count > 0)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.InvalidRequest, errors);
        }

        return await next(cancellationToken);
    }
}
