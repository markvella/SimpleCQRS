using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleCQRS.Client
{
    public interface IClient<TRequest, TResponse> : IDisposable
    {
        Task<TResponse> RequestAsync(TRequest Request, CancellationToken ct);
    }
}