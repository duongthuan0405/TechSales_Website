using System;

namespace TechSalesManagement.Application.Services.Params;

public class CreateAddressParams
{
    public required Guid UserId { get; set; }
    public required string Province { get; set; }
    public required string Ward { get; set; }
    public required string Detail { get; set; }
}

public class UpdateAddressParams
{
    public required Guid AddressId { get; set; }
    public required Guid UserId { get; set; }
    public required string Province { get; set; }
    public required string Ward { get; set; }
    public required string Detail { get; set; }
}

public class SetDefaultAddressParams
{
    public required Guid AddressId { get; set; }
    public required Guid UserId { get; set; }
}

public class DeleteAddressParams
{
    public required Guid AddressId { get; set; }
    public required Guid UserId { get; set; }
}
