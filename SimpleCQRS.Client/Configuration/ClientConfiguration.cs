using System;
using System.Collections.Generic;
using SimpleCQRS.Contracts;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Client.Configuration
{
    public class ClientConfiguration : IClientConfiguration
    {
        internal string ServiceName { get; private set; }
        internal ConnectionConfiguration Connection { get; private set; }
        internal ISerializer Serializer { get; private set; }
        
        public ClientConfiguration()
        {
            Serializer = new NullSerializer();
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

        public IClientConfiguration UseService(string serviceName)
        {
            ServiceName = serviceName;
            return this;
        }

        public IClientConfiguration Using(ISerializer serializer)
        {
            Serializer = serializer;
            return this;
        }
    }
}