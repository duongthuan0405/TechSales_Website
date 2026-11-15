using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<(System.Collections.Generic.List<AuditLog> items, int totalCount)> GetPagedLogsAsync(int pageNumber, int pageSize, Guid? userId = null);
}
