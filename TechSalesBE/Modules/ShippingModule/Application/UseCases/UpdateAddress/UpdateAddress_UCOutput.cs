namespace ShippingModule.Application.UseCases.UpdateAddress;

internal sealed record UpdateAddress_UCOutput(Guid Id, string ProvinceCode, string? ProvinceName, string WardCode, string? WardName,
    string Detail, bool IsDefault, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
