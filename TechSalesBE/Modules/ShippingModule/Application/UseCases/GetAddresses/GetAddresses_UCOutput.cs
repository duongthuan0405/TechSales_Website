namespace ShippingModule.Application.UseCases.GetAddresses;

internal sealed record GetAddresses_UCOutput(List<GetAddresses_UCOutput.AddressItem> Items)
{
    internal sealed record AddressItem(Guid Id, string ProvinceCode, string? ProvinceName, string WardCode, string? WardName,
    string Detail, bool IsDefault, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
}
