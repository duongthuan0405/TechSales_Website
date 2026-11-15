using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IVoucherRepository
{
    Task AddAsync(Voucher voucher);
    Task UpdateVoucherAsync(Voucher voucher);
    Task DeleteAsync(Guid id);
    Task<Voucher?> GetByIdAsync(Guid id);
    Task<Voucher?> GetByCodeAsync(string code);
    Task<(List<Voucher> items, int totalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<bool> ExistsByCodeAsync(string code);
}
