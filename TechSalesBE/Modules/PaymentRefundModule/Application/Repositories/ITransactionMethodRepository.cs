using PaymentRefundModule.Domain.Entities;

namespace PaymentRefundModule.Application.Repositories;

internal interface ITransactionMethodRepository
{
    // Supported methods are provisioned with gateway integrations, not created by administrators.
    Task<List<TransactionMethod>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TransactionMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
