using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IAuditService
{
    Task<(List<AuditLog> items, int totalCount)> GetSystemLogsAsync(int pageNumber, int pageSize, Guid? userId = null);
}
