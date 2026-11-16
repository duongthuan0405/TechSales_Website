using AfterSaleModule.Domain.Enums;

namespace AfterSaleModule.Infrastructure.Persistence.Models;

internal sealed class RepairResultDbModel
{
    public Guid Id { get; set; }
    public RepairType RepairType { get; set; }
    public RepairProcessingLocation? ProcessingLocation { get; set; }
    public string? CheckingResult { get; set; }
    public string? SolutionDescription { get; set; }
    public DateTimeOffset? TransferredToManufactureAt { get; set; }
    public DateTimeOffset? ReturnedToStoreAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public long TotalAmount { get; set; }

    public RepairRequestDbModel RepairRequest { get; set; } = null!;
}
