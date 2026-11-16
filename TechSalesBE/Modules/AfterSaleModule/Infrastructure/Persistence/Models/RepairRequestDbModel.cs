using AfterSaleModule.Domain.Enums;

namespace AfterSaleModule.Infrastructure.Persistence.Models;

internal sealed class RepairRequestDbModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public RepairRequestStatus Status { get; set; }
    public string? ProblemDescription { get; set; }
    public DateTimeOffset? ReceivedDeviceAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public RepairResultDbModel? Result { get; set; }
}
