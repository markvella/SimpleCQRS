using System;
using System.Collections.Generic;

namespace SimpleCQRS.Host.Configuration
{
    public class HostConfiguration : IHostConfiguration
    {
        internal string ServiceName { get; private set; }
        internal ConnectionConfiguration Connection { get; private set; }
        internal List<OperationConfiguration> Operations { get; private set; }

        public HostConfiguration()
        {
            Operations = new List<OperationConfiguration>();
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

        public IHostConfiguration AddOperation<T>(string operationName, Action<T> handler)
        {
            Operations.Add(new OperationConfiguration<T>(operationName, handler));
            return this;
        }

        public IHostConfiguration SetService(string serviceName)
        {
            ServiceName = serviceName;
            return this;
        }
    }
}
