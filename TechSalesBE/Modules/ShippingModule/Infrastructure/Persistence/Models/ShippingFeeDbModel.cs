namespace ShippingModule.Infrastructure.Persistence.Models;

internal sealed class ShippingFeeDbModel
{
    public string ProviceCode { get; set; } = string.Empty;
    public long Fee { get; set; }
}
