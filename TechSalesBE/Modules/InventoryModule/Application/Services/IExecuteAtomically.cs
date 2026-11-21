namespace InventoryModule.Application.Services;

internal interface IExecuteAtomically
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken);
}
