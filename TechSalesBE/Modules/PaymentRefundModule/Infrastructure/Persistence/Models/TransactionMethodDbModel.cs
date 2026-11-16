using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Infrastructure.Persistence.Models;

internal sealed class TransactionMethodDbModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public TransactionMethodType? Type { get; set; }

    public ICollection<PaymentDbModel> Payments { get; set; } = new HashSet<PaymentDbModel>();
}
