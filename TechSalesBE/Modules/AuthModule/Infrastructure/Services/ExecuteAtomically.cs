using AuthModule.Application.Services;

namespace AuthModule.Infrastructure.Services;

public class ExecuteAtomically : IExecuteAtomically
{
    public async Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask)
    {
        try
        {
            return await mainTask.Invoke();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
