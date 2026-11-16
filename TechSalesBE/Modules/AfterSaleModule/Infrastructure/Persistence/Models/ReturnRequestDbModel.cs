using AfterSaleModule.Domain.Enums;

namespace AfterSaleModule.Infrastructure.Persistence.Models;

internal sealed class ReturnRequestDbModel
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid ReasonId { get; set; }
    public string? ReasonDescription { get; set; }
    public ReturnRequestStatus? Status { get; set; }

    public ReturnRequestReasonDbModel Reason { get; set; } = null!;
    public ICollection<ReturnRequestImageDbModel> Images { get; set; } = new HashSet<ReturnRequestImageDbModel>();
    public ReturnResultDbModel? Result { get; set; }
}
