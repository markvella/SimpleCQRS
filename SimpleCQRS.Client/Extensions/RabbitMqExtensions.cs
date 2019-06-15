using RabbitMQ.Client;
using SimpleCQRS.Client.Configuration;

namespace SimpleCQRS.Client.Extensions
{
    internal static class RabbitMqExtensions
    {
        internal static IConnection CreateConnection(this ClientConfiguration config)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = config.Connection.HostName,
                Port = config.Connection.Port,
                VirtualHost = config.Connection.VirtualHost,
                UserName = config.Connection.UserName,
                Password = config.Connection.Password
            };

            return connectionFactory.CreateConnection();
        }
    }
}
