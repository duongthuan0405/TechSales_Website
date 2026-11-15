using System.Linq;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly TechSalesDbContext _dbContext;

    public AuditLogRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog log)
    {
        var dbModel = new AuditLogDbModel
        {
            id = log.id != System.Guid.Empty ? log.id : System.Guid.NewGuid(),
            user_id = log.userId,
            action = log.action,
            table_name = log.tableName,
            primary_key = log.primaryKey,
            old_values = log.oldValues,
            new_values = log.newValues,
            affected_columns = log.affectedColumns,
            created_at = log.createdAt
        };

        await _dbContext.AuditLogs.AddAsync(dbModel);
    }

    public async Task<(System.Collections.Generic.List<AuditLog> items, int totalCount)> GetPagedLogsAsync(int pageNumber, int pageSize, Guid? userId = null)
    {
        var query = _dbContext.AuditLogs.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(l => l.user_id == userId.Value);
        }

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(l => l.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = dbModels.Select(m => new AuditLog
        {
            id = m.id,
            userId = m.user_id,
            action = m.action,
            tableName = m.table_name,
            primaryKey = m.primary_key,
            oldValues = m.old_values,
            newValues = m.new_values,
            affectedColumns = m.affected_columns,
            createdAt = m.created_at
        }).ToList();

        return (items, totalCount);
    }
}
