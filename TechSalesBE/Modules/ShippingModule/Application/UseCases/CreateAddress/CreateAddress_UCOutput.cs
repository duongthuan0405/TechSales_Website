namespace ShippingModule.Application.UseCases.CreateAddress;

internal sealed record CreateAddress_UCOutput(Guid Id, string ProvinceCode, string? ProvinceName, string WardCode, string? WardName,
    string Detail, bool IsDefault, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
