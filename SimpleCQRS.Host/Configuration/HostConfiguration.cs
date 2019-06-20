using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleCQRS.Contracts;
using SimpleCQRS.Loggers;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host.Configuration
{
    public class HostConfiguration : IHostConfiguration
    {
        internal string ServiceName { get; private set; }
        internal ConnectionConfiguration Connection { get; private set; }
        internal List<OperationConfiguration> Operations { get; private set; }

        internal ISerializer Serializer { get; private set; }
        internal ILogger Logger { get; private set; }

        public HostConfiguration()
        {
            Operations = new List<OperationConfiguration>();
            Serializer = new NullSerializer();
            Logger = new NullLogger();
        }

        public IHostConfiguration ConnectTo(
            string hostName = "localhost",
            int port = 5672,
            string virtualHost = "/",
            string username = "guest",
            string password = "guest")
        {
            Connection = new ConnectionConfiguration(hostName, port, virtualHost, username, password);
            return this;
        }

        public IHostConfiguration AddOperation<TRequest, TResponse>(string operationName, Func<Envelope<TRequest>, IHostOperation<TRequest, TResponse>, Task> handler)
        {
            Operations.Add(new OperationConfiguration<TRequest, TResponse>(operationName, handler));
            return this;
        }

        public IHostConfiguration SetService(string serviceName)
        {
            ServiceName = serviceName;
            return this;
        }

        public IHostConfiguration Using(ISerializer serializer)
        {
            Serializer = serializer;
            return this;
        }

        public IHostConfiguration Using(ILogger logger)
        {
            Logger = logger;
            return this;
        }
    }
}
