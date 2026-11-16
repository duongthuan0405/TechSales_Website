namespace AfterSaleModule.Infrastructure.Persistence.Models;

internal sealed class ReturnResultProductUnitDbModel
{
    public Guid ReturnResultId { get; set; }
    public Guid ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? ResultCheckingDescription { get; set; }
}
