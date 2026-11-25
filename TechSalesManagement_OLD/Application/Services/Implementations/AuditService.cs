using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<(List<AuditLog> items, int totalCount)> GetSystemLogsAsync(int pageNumber, int pageSize, Guid? userId = null)
    {
        return await _auditLogRepository.GetPagedLogsAsync(pageNumber, pageSize, userId);
    }
}
