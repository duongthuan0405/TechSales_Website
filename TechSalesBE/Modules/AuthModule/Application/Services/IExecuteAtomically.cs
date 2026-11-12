using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthModule.Application.Services
{
    public interface IExecuteAtomically
    {
        public Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask);
    }
}