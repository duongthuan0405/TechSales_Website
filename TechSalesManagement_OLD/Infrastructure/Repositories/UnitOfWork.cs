using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Infrastructure.Persistence;

namespace TechSalesManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TechSalesDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginAsync()
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task FinishAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
