using System;
using SimpleCQRS.Loggers;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Client.Configuration
{
    public class ClientConfiguration : IClientConfiguration
    {
        internal string ServiceName { get; private set; }
        internal string OperationName { get; private set; }
        internal ConnectionConfiguration Connection { get; private set; }
        internal ISerializer Serializer { get; private set; }
        internal int Retries { get; private set; }
        internal TimeSpan MaximumTimeout { get; private set; }
        internal int PublishingPoolSize { get; private set; }
        internal int ConsumingPoolSize { get; private set; }
        internal ILogger Logger { get; private set; }
        
        public ClientConfiguration()
        {
            Serializer = new NullSerializer();
            Retries = 0;
            MaximumTimeout = TimeSpan.MaxValue;
            PublishingPoolSize = 1;
            ConsumingPoolSize = 1;
            Logger = new NullLogger();
        }

        public IClientConfiguration ConnectTo(
            string hostName = "localhost",
            int port = 5672,
            string virtualHost = "/",
            string username = "guest",
            string password = "guest")
        {
            Connection = new ConnectionConfiguration(hostName, port, virtualHost, username, password);
            return this;
        }

        public IClientConfiguration ForOperation(string serviceName, string operationName)
        {
            ServiceName = serviceName;
            OperationName = operationName;
            return this;
        }

        public IClientConfiguration SetRetries(int retries)
        {
            Retries = retries;
            return this;
        }
        
        public IClientConfiguration SetMaximumTimeout(TimeSpan timeout)
        {
            MaximumTimeout = timeout;
            return this;
        }

        public IClientConfiguration SetPoolingSize(int publishingPoolSize, int consumingPoolSize)
        {
            PublishingPoolSize = publishingPoolSize;
            ConsumingPoolSize = consumingPoolSize;
            return this;
        }
        
        public IClientConfiguration Using(ISerializer serializer)
        {
            Serializer = serializer;
            return this;
        }
        
        public IClientConfiguration Using(ILogger logger)
        {
            Logger = logger;
            return this;
        }
    }
}