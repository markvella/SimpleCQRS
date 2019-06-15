using System;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Client.Configuration
{
    public interface IClientConfiguration
    {
        IClientConfiguration ConnectTo(
            string hostName = "localhost",
            int port = 5672,
            string virtualHost = "/",
            string username = "guest",
            string password = "guest");

        IClientConfiguration ForOperation(string serviceName, string operationName);

        IClientConfiguration SetMaximumTimeout(TimeSpan timeout);
        
        IClientConfiguration SetPoolingSize(int publishingPoolSize, int consumingPoolSize);
        
        IClientConfiguration Using(ISerializer serializer);
    }
}