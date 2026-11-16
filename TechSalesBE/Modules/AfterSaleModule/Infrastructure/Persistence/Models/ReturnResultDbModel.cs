namespace AfterSaleModule.Infrastructure.Persistence.Models;

internal sealed class ReturnResultDbModel
{
    public Guid Id { get; set; }
    public int ApprovedQuantity { get; set; }
    public string? ResultCheckingDescription { get; set; }
    public long TotalRefundAmount { get; set; }

    public ReturnRequestDbModel ReturnRequest { get; set; } = null!;
}
