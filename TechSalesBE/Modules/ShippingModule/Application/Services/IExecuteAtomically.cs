namespace ShippingModule.Application.Services;

internal interface IExecuteAtomically
{
    Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default);
}
