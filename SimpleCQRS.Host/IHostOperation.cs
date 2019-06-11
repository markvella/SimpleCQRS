using System;
using SimpleCQRS.Contracts;

namespace SimpleCQRS.Host
{
    public interface IHostOperation<TRequest, TResponse> : IDisposable
    {
        void SendReply(Envelope<TRequest> env, object reply);
    }
}