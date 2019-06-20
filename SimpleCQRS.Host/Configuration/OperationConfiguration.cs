using System;
using System.Threading.Tasks;
using SimpleCQRS.Contracts;

namespace SimpleCQRS.Host.Configuration
{
    internal class OperationConfiguration<TRequest, TResponse> : OperationConfiguration
    {
        internal OperationConfiguration(string operationName, Func<Envelope<TRequest>, IHostOperation<TRequest, TResponse>, Task> handler)
            : base(operationName, typeof(TRequest), typeof(TResponse))
        {
            Handler = handler;
        }
        
        internal Func<Envelope<TRequest>, IHostOperation<TRequest, TResponse>, Task> Handler { get; }
    }
    
    internal class OperationConfiguration
    {
        internal OperationConfiguration(string operationName, Type requestType, Type responseType)
        {
            OperationName = operationName;
            RequestType = requestType;
            ResponseType = responseType;
        }

        internal string OperationName { get; }

        internal Type RequestType { get; }

        internal Type ResponseType { get; }
    }
}
