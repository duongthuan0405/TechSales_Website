using CommonModule.BusinessExceptions;
using FluentValidation;
using MediatR;

namespace AuthModule.Application.Behaviors;
internal sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, List<string>>();
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            foreach (var failure in result.Errors)
            {
                if (!errors.TryGetValue(failure.PropertyName, out var list))
                {
                    errors[failure.PropertyName] = list = [];
                }

                list.Add(failure.ErrorMessage);
            }
        }

        if (errors.Count > 0)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "Invalid request.", errors);
        }

        return await next(cancellationToken);
    }
}
