using System;
using System.Threading.Tasks;
using SimpleCQRS.Contracts;

namespace SimpleCQRS.Host.Configuration
{
    internal class OperationConfiguration<TRequest, TResponse> : OperationConfiguration
    {
        internal OperationConfiguration(
            string operationName,
            Func<Envelope<TRequest>,
            IHostOperation<TRequest, TResponse>, Task> handler, 
            int numberOfConsumers = 10)
            : base(operationName, typeof(TRequest), typeof(TResponse), numberOfConsumers)
        {
            Handler = handler;
        }
        
        internal Func<Envelope<TRequest>, IHostOperation<TRequest, TResponse>, Task> Handler { get; }
    }
    
    internal class OperationConfiguration
    {
        internal OperationConfiguration(string operationName, Type requestType, Type responseType, int numberOfConsumers)
        {
            OperationName = operationName;
            RequestType = requestType;
            ResponseType = responseType;
            NumberofOfConsumers = numberOfConsumers;
        }

        internal string OperationName { get; }

        internal Type RequestType { get; }

        internal Type ResponseType { get; }
        
        internal int NumberofOfConsumers { get; }
    }
}
