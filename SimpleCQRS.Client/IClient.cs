using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleCQRS.Client.Enumerations;

namespace SimpleCQRS.Client
{
    public interface IClient<in TRequest, TResponse> : IDisposable
    {
        Task<TResponse> RequestAsync(TRequest request, CancellationToken ct, Priority priority = Priority.Normal);
    }
}