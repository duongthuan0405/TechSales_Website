using FluentValidation;

namespace ShippingModule.Application.UseCases.SetDefaultAddress;

internal sealed class SetDefaultAddress_UCInput_Validator : AbstractValidator<SetDefaultAddress_UCInput>
{
    public SetDefaultAddress_UCInput_Validator()
    {
        RuleFor(input => input.ActorId).NotEmpty();
        RuleFor(input => input.AddressId).NotEmpty();
    }
}
