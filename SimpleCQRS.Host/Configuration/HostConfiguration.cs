using System;
using System.Collections.Generic;
using SimpleCQRS.Contracts;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host.Configuration
{
    public class HostConfiguration : IHostConfiguration
    {
        internal string ServiceName { get; private set; }
        internal ConnectionConfiguration Connection { get; private set; }
        internal List<OperationConfiguration> Operations { get; private set; }

        internal ISerializer Serializer { get; private set; }
        
        public HostConfiguration()
        {
            Operations = new List<OperationConfiguration>();
            Serializer = new NullSerializer();
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

        public IHostConfiguration AddOperation<TRequest, TResponse>(string operationName, Action<Envelope<TRequest>, IHostOperation<TRequest, TResponse>> handler)
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
    }
}
