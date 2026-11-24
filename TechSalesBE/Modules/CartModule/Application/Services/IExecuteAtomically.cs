namespace CartModule.Application.Services;

internal interface IExecuteAtomically
{
    Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken);
}
