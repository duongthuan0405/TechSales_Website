namespace ShippingModule.Infrastructure.Persistence.Models;

internal sealed class WardDbModel
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string ProvinceCode { get; set; } = string.Empty;

    public ProvinceDbModel Province { get; set; } = null!;
    public ICollection<ShippingAddressDbModel> ShippingAddresses { get; set; } = new HashSet<ShippingAddressDbModel>();
}
