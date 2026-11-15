using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Implementations;

public class VoucherManagementService : IVoucherManagementService
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VoucherManagementService(
        IVoucherRepository voucherRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _voucherRepository = voucherRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Voucher> CreateVoucherAsync(string code, VoucherType type, decimal value, int maxUsage, decimal minOrderAmount, DateTimeOffset? startDate, DateTimeOffset? endDate, Guid staffId)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new BadRequestException("Voucher code is required.");
        
        var exists = await _voucherRepository.ExistsByCodeAsync(code);
        if (exists) throw new BadRequestException("Voucher code already exists.");

        // Basic Strategy Validation
        if (type == VoucherType.PERCENT && value > 100)
            throw new BadRequestException("Percentage cannot exceed 100%.");

        try
        {
            await _unitOfWork.BeginAsync();

            var voucher = new Voucher(code, type, value)
            {
                maxUsage = maxUsage,
                minOrderAmount = minOrderAmount,
                startDate = startDate,
                endDate = endDate
            };

            await _voucherRepository.AddAsync(voucher);

            var auditLog = new AuditLog(staffId, "CREATE_VOUCHER", "Vouchers", code);
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
            return voucher;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteVoucherAsync(Guid id, Guid staffId)
    {
        var voucher = await _voucherRepository.GetByIdAsync(id);
        if (voucher == null) throw new NotFoundException("Voucher not found.");

        try
        {
            await _unitOfWork.BeginAsync();

            voucher.isActive = false;
            await _voucherRepository.UpdateVoucherAsync(voucher);

            var auditLog = new AuditLog(staffId, "DEACTIVATE_VOUCHER", "Vouchers", voucher.code)
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { isActive = true }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { isActive = false }),
                affectedColumns = "isActive"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<(List<Voucher> items, int totalCount)> GetAllVouchersAsync(int pageNumber, int pageSize)
    {
        return await _voucherRepository.GetAllPagedAsync(pageNumber, pageSize);
    }

    public async Task<Voucher> ValidateVoucherAsync(string code, decimal orderAmount)
    {
        var voucher = await _voucherRepository.GetByCodeAsync(code);
        if (voucher == null) throw new BadRequestException(MessageConstants.MSG33);

        if (!voucher.isActive) throw new BadRequestException(MessageConstants.MSG33);
        if (voucher.usedCount >= voucher.maxUsage) throw new BadRequestException(MessageConstants.MSG33);
        if (orderAmount < voucher.minOrderAmount) throw new BadRequestException(MessageConstants.MSG33);

        var now = DateTimeOffset.UtcNow;
        if (voucher.startDate.HasValue && voucher.startDate.Value > now) throw new BadRequestException(MessageConstants.MSG33);
        if (voucher.endDate.HasValue && voucher.endDate.Value < now) throw new BadRequestException(MessageConstants.MSG33);

        return voucher;
    }
}
