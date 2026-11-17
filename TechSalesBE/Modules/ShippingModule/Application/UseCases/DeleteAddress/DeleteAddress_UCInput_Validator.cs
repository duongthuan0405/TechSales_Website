using FluentValidation;

namespace ShippingModule.Application.UseCases.DeleteAddress;

internal sealed class DeleteAddress_UCInput_Validator : AbstractValidator<DeleteAddress_UCInput>
{
    public DeleteAddress_UCInput_Validator()
    {
        RuleFor(input => input.ActorId).NotEmpty();
        RuleFor(input => input.AddressId).NotEmpty();
    }
}
