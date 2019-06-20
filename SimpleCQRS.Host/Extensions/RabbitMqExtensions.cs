using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Host.Configuration;

namespace SimpleCQRS.Host.Extensions
{
    internal static class RabbitMqExtensions
    {
        internal static IConnection CreateConnection(this HostConfiguration config)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = config.Connection.HostName,
                Port = config.Connection.Port,
                VirtualHost = config.Connection.VirtualHost,
                UserName = config.Connection.UserName,
                Password = config.Connection.Password,
                DispatchConsumersAsync = true
            };

            return connectionFactory.CreateConnection();
        }
    }
}
