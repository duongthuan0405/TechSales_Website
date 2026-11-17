namespace AuthModule.Application.Services;
public interface IExecuteAtomically
{
    Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default);
}
