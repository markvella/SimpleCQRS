using System;
using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public interface IRequestHandler<T,TResponse>:IRequestHandler
    {
        Task<TResponse> Process(Envelope<T> request);
    }
    public interface IRequestHandler
    {

    }
}
