using AuthModule.Domain.Constants;
using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.GetStaff;
internal sealed class GetStaff_UCInput_Validator : AbstractValidator<GetStaff_UCInput>
{
    public GetStaff_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.PageNumber).InclusiveBetween(AuthLimits.MinPageNumber, AuthLimits.MaxPageNumber);
        RuleFor(x => x.PageSize).InclusiveBetween(AuthLimits.MinPageSize, AuthLimits.MaxPageSize);
    }
}
