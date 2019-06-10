using System;
using SimpleCQRS.Contracts;

namespace SimpleCQRS.Host.Configuration
{
    internal class OperationConfiguration<TRequest, TResponse> : OperationConfiguration
    {
        internal OperationConfiguration(string operationName, Action<Envelope<TRequest>, IHostOperation<TRequest, TResponse>> handler) : base(operationName, typeof(TRequest), typeof(TResponse))
        {
            Handler = handler;
        }
        
        internal Action<Envelope<TRequest>, IHostOperation<TRequest, TResponse>> Handler { get; }
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
