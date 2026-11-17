using ShippingModule.Domain.Rules;

namespace ShippingModule.Domain.Entities;

internal sealed class ShippingFee
{
    public string ProvinceCode { get; }
    public long Fee { get; }

    public ShippingFee(string provinceCode, long fee)
    {
        ShippingFeeRules.EnsureValid(provinceCode, fee);

        ProvinceCode = provinceCode;
        Fee = fee;
    }
}
