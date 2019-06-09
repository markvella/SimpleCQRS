using System;

namespace SimpleCQRS.Host.Configuration
{
    public interface IHostConfiguration
    {
        IHostConfiguration ConnectTo(
            string hostName = "localhost",
            int port = 5672,
            string virtualHost = "/",
            string username = "guest",
            string password = "guest");

        IHostConfiguration SetService(string serviceName);

        IHostConfiguration AddOperation<T>(string operationName, Action<T> handler);
    }
}
