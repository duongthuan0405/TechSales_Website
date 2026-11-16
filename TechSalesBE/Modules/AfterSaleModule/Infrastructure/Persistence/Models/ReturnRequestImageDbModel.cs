namespace AfterSaleModule.Infrastructure.Persistence.Models;

internal sealed class ReturnRequestImageDbModel
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public Guid ReturnRequestId { get; set; }

    public ReturnRequestDbModel ReturnRequest { get; set; } = null!;
}
