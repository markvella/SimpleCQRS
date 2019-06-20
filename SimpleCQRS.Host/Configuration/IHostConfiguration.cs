using System;
using System.Threading.Tasks;
using SimpleCQRS.Contracts;
using SimpleCQRS.Loggers;
using SimpleCQRS.Serializers;

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

        IHostConfiguration AddOperation<TRequest, TResponse>(string operationName, Func<Envelope<TRequest>, IHostOperation<TRequest, TResponse>, Task> handler);

        IHostConfiguration Using(ISerializer serializer);

        IHostConfiguration Using(ILogger logger);
    }
}
