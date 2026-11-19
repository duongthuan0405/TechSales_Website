using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.Repositories;

internal interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        PaymentType? type, TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default);
}
