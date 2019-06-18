using System;
using System.Linq;
using SimpleCQRS.Contracts.Exceptions;
using SimpleCQRS.Host.Configuration;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host
{
    public static class HostFactory
    {
        public static IHost Create(Action<IHostConfiguration> configure)
        {
            var config = new HostConfiguration();
            configure(config);

            if (string.IsNullOrEmpty(config.ServiceName))
            {
                throw new SimpleCQRSException($"{nameof(config.ServiceName)} must be provided.");
            }

            if (!config.Operations.Any())
            {
                throw new SimpleCQRSException("At least 1 operation must be provided.");
            }

            config.Operations.ForEach(o =>
            {
                if (string.IsNullOrEmpty(o.OperationName))
                {
                    throw new SimpleCQRSException($"{nameof(o.OperationName)} must be provided.");
                }
            });
            
            if (config.Serializer is NullSerializer)
            {
                throw new SimpleCQRSException($"A valid serializer must be provided.");
            }
            
            return new CQRSHost(config);
        }
    }
}
