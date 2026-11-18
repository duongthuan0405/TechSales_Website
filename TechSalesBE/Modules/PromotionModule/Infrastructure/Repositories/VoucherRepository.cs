using Microsoft.EntityFrameworkCore;
using PromotionModule.Application.Repositories;
using PromotionModule.Domain.Entities;
using PromotionModule.Infrastructure.Persistence;
using PromotionModule.Infrastructure.Persistence.Models;

namespace PromotionModule.Infrastructure.Repositories;

internal sealed class VoucherRepository : IVoucherRepository
{
    private readonly PromotionDbContext _db;

    public VoucherRepository(PromotionDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Voucher> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        int count = await _db.Vouchers.CountAsync(cancellationToken);
        List<VoucherDbModel> rows = await _db.Vouchers.AsNoTracking().OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        List<Voucher> items = new List<Voucher>();
        foreach (VoucherDbModel row in rows)
        {
            items.Add(Map(row));
        }

        return (items, count);
    }

    public async Task<Voucher?> GetByIdAsync(Guid id, bool forUpdate, CancellationToken cancellationToken)
    {
        IQueryable<VoucherDbModel> query = _db.Vouchers;
        if (forUpdate && _db.Database.IsNpgsql())
        {
            query = _db.Vouchers.FromSqlInterpolated($"SELECT * FROM promotion.\"Voucher\" WHERE \"Id\" = {id} FOR UPDATE");
        }

        VoucherDbModel? row = await query.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return Map(row);
    }

    public async Task<Voucher?> GetByCodeAsync(string normalizedCode, CancellationToken cancellationToken)
    {
        VoucherDbModel? row = await _db.Vouchers.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Code.Trim().ToUpper() == normalizedCode, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return Map(row);
    }

    public async Task LockCodeAsync(string normalizedCode, CancellationToken cancellationToken)
    {
        if (_db.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Voucher writes require an active transaction.");
        }

        if (_db.Database.IsNpgsql())
        {
            string lockKey = $"promotion-voucher:{normalizedCode}";
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", cancellationToken);
        }
    }

    public async Task SaveAsync(Voucher voucher, CancellationToken cancellationToken)
    {
        VoucherDbModel? row = await _db.Vouchers.SingleOrDefaultAsync(item => item.Id == voucher.Id, cancellationToken);
        if (row == null)
        {
            row = new VoucherDbModel { Id = voucher.Id };
            _db.Vouchers.Add(row);
        }

        row.Code = voucher.Code;
        row.Type = voucher.Type;
        row.Value = voucher.Value;
        row.MaxUsage = voucher.MaxUsage;
        row.UsedCount = voucher.UsedCount;
        row.MinOrderAmount = voucher.MinOrderAmount;
        row.StartDate = voucher.StartDate;
        row.EndDate = voucher.EndDate;
        row.IsActive = voucher.IsActive;
        row.CreatedAt = voucher.CreatedAt;
        row.UpdatedAt = voucher.UpdatedAt;
    }

    private static Voucher Map(VoucherDbModel row)
    {
        return new Voucher(row.Id, row.Code, row.Type, row.Value, row.MaxUsage, row.UsedCount ?? 0,
            row.MinOrderAmount, row.StartDate, row.EndDate, row.IsActive, row.CreatedAt, row.UpdatedAt);
    }

    public async Task<(List<Voucher> Items, int TotalCount)> GetPageWithCanBeUsedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {

        IQueryable<VoucherDbModel> query = _db.Vouchers.AsNoTracking().Where(v => v.IsActive && v.EndDate > DateTimeOffset.UtcNow).OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize);
        
        List<VoucherDbModel> rows =  await query.ToListAsync(cancellationToken);
        int count = await query.CountAsync();
        List<Voucher> items = new List<Voucher>();
        foreach (VoucherDbModel row in rows)
        {
            items.Add(Map(row));
        }

        return (items, count);
    }
}
