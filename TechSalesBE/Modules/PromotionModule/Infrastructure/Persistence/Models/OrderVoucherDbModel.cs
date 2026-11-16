namespace PromotionModule.Infrastructure.Persistence.Models;

internal sealed class OrderVoucherDbModel
{
    public Guid OrderId { get; set; }
    public Guid VoucherId { get; set; }
}
