using PromotionModule.Domain.Entities;

namespace PromotionModule.Application.Repositories;

internal interface IVoucherRepository
{
    Task<(List<Voucher> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Voucher?> GetByIdAsync(Guid id, bool forUpdate, CancellationToken cancellationToken);
    Task<Voucher?> GetByCodeAsync(string normalizedCode, CancellationToken cancellationToken);
    Task LockCodeAsync(string normalizedCode, CancellationToken cancellationToken);
    Task SaveAsync(Voucher voucher, CancellationToken cancellationToken);
}
