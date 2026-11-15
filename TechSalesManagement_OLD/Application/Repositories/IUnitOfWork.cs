using System.Threading.Tasks;

namespace TechSalesManagement.Application.Interfaces;

public interface IUnitOfWork
{
    Task BeginAsync();
    Task FinishAsync();
    Task RollbackAsync();
}
