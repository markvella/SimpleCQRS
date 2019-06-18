using System;
using SimpleCQRS.Client.Configuration;
using SimpleCQRS.Contracts.Exceptions;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Client
{
    public static class ClientFactory
    {
        public static IClient<TRequest, TResponse> Create<TRequest, TResponse>(Action<IClientConfiguration> configure)
        {
            var config = new ClientConfiguration();
            configure(config);

            if (string.IsNullOrEmpty(config.ServiceName))
            {
                throw new SimpleCQRSException($"{nameof(config.ServiceName)} must be provided.");
            }
            
            if (string.IsNullOrEmpty(config.OperationName))
            {
                throw new SimpleCQRSException($"{nameof(config.OperationName)} must be provided.");
            }
            
            if (config.Serializer is NullSerializer)
            {
                throw new SimpleCQRSException($"A valid serializer must be provided.");
            }
            
            return new CQRSClient<TRequest, TResponse>(config);
        }
    }
}