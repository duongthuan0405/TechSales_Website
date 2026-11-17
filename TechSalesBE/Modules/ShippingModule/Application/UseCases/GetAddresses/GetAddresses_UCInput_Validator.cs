using FluentValidation;

namespace ShippingModule.Application.UseCases.GetAddresses;

internal sealed class GetAddresses_UCInput_Validator : AbstractValidator<GetAddresses_UCInput>
{
    public GetAddresses_UCInput_Validator()
    {
        RuleFor(input => input.ActorId).NotEmpty();
    }
}
