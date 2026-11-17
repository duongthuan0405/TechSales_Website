using ShippingModule.Infrastructure.Persistence.Models;

namespace ShippingModule.Infrastructure.Persistence;

internal static class ShippingSeedData
{
    internal const long InitialShippingFee = 10_000L;

    internal static readonly List<ShippingFeeDbModel> ShippingFees = new List<ShippingFeeDbModel>
    {
        new ShippingFeeDbModel { ProviceCode = "11", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "12", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "13", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "14", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "15", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "16", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "17", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "18", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "19", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "20", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "21", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "22", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "23", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "24", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "25", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "26", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "27", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "28", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "29", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "30", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "31", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "32", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "33", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "34", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "35", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "36", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "37", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "38", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "39", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "40", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "41", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "42", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "43", Fee = InitialShippingFee },
        new ShippingFeeDbModel { ProviceCode = "44", Fee = InitialShippingFee },
    };
}
