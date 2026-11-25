using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IVoucherManagementService
{
    Task<Voucher> CreateVoucherAsync(string code, VoucherType type, decimal value, int maxUsage, decimal minOrderAmount, DateTimeOffset? startDate, DateTimeOffset? endDate, Guid staffId);
    Task DeleteVoucherAsync(Guid id, Guid staffId);
    Task<(List<Voucher> items, int totalCount)> GetAllVouchersAsync(int pageNumber, int pageSize);
    Task<Voucher> ValidateVoucherAsync(string code, decimal orderAmount);
}
