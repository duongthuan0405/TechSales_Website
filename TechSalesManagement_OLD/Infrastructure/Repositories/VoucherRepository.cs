using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class VoucherRepository : IVoucherRepository
{
    private readonly TechSalesDbContext _dbContext;

    public VoucherRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Voucher voucher)
    {
        var dbModel = new VoucherDbModel
        {
            id = Guid.NewGuid(),
            code = voucher.code,
            type = voucher.type,
            value = voucher.value,
            max_usage = voucher.maxUsage,
            used_count = 0,
            min_order_amount = voucher.minOrderAmount,
            start_date = voucher.startDate?.ToUniversalTime(),
            end_date = voucher.endDate?.ToUniversalTime(),
            is_active = true,
            created_at = DateTimeOffset.UtcNow,
            updated_at = DateTimeOffset.UtcNow
        };
        await _dbContext.Vouchers.AddAsync(dbModel);
    }

    public async Task UpdateVoucherAsync(Voucher voucher)
    {
        var dbModel = await _dbContext.Vouchers.FindAsync(voucher.id);
        if (dbModel != null)
        {
            dbModel.code = voucher.code;
            dbModel.used_count = voucher.usedCount;
            dbModel.is_active = voucher.isActive;
            dbModel.updated_at = DateTimeOffset.UtcNow;
            _dbContext.Vouchers.Update(dbModel);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var dbModel = await _dbContext.Vouchers.FindAsync(id);
        if (dbModel != null)
        {
            dbModel.is_active = false; // Soft delete
            dbModel.updated_at = DateTimeOffset.UtcNow;
            _dbContext.Vouchers.Update(dbModel);
        }
    }

    public async Task<Voucher?> GetByIdAsync(Guid id)
    {
        var dbModel = await _dbContext.Vouchers.FindAsync(id);
        return MapToEntity(dbModel);
    }

    public async Task<Voucher?> GetByCodeAsync(string code)
    {
        var dbModel = await _dbContext.Vouchers
            .FirstOrDefaultAsync(v => v.code == code && v.is_active);
        return MapToEntity(dbModel);
    }

    public async Task<(List<Voucher> items, int totalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbContext.Vouchers.AsQueryable();
        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(v => v.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (dbModels.Select(m => MapToEntity(m)!).ToList(), totalCount);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbContext.Vouchers.AnyAsync(v => v.code == code);
    }

    private Voucher? MapToEntity(VoucherDbModel? dbModel)
    {
        if (dbModel == null) return null;
        return new Voucher
        {
            id = dbModel.id,
            code = dbModel.code,
            type = dbModel.type,
            value = dbModel.value,
            maxUsage = dbModel.max_usage,
            usedCount = dbModel.used_count,
            minOrderAmount = dbModel.min_order_amount,
            startDate = dbModel.start_date,
            endDate = dbModel.end_date,
            isActive = dbModel.is_active,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at
        };
    }
}
