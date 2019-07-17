using System;
using SimpleCQRS.Contracts;

namespace SimpleCQRS.Host
{
    public interface IHostOperation<TRequest, in TResponse> : IDisposable
    {
        void SendReply(Envelope<TRequest> env, TResponse reply);
    }
}