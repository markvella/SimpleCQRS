using System;
using SimpleCQRS.Client.Configuration;

namespace SimpleCQRS.Client
{
    public class ClientFactory
    {
        public static IClient<TRequest, TResponse> Create<TRequest, TResponse>(Action<IClientConfiguration> configure)
        {
            var config = new ClientConfiguration();
            configure(config);

            return new CQRSClientV2<TRequest, TResponse>(config);
        }
    }
}