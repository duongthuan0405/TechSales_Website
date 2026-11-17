using ShippingModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using MediatR;
using ShippingModule.Application.Repositories;
using ShippingModule.Application.Services;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.UseCases.SetShippingFee;

internal sealed class SetShippingFee_UCHandler : IRequestHandler<SetShippingFee_UCInput, SetShippingFee_UCOutput>
{
    private readonly ILocationRepository _locations;
    private readonly IShippingFeeRepository _fees;
    private readonly IExecuteAtomically _transaction;

    public SetShippingFee_UCHandler(ILocationRepository locations, IShippingFeeRepository fees,
        IExecuteAtomically transaction)
    {
        _locations = locations;
        _fees = fees;
        _transaction = transaction;
    }

    public async Task<SetShippingFee_UCOutput> Handle(SetShippingFee_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Province? province = await _locations.GetProvinceAsync(input.ProvinceCode, cancellationToken);
            if (province == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProvinceNotFound);
            }

            ShippingFee shippingFee = new ShippingFee(province.Code, input.Fee);
            await _fees.SaveAsync(shippingFee, cancellationToken);
            return new SetShippingFee_UCOutput(shippingFee.ProvinceCode, shippingFee.Fee);
        }, cancellationToken);
    }
}
