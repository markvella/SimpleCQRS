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
                Password = config.Connection.Password
            };

            return connectionFactory.CreateConnection();
        }

        internal static string GetHeaderValue(this BasicDeliverEventArgs eventArgs, string headerName)
        {
            const string defaultValue = null;
            var headerValue = eventArgs?.BasicProperties?.Headers[headerName];

            if (headerName == null)
            {
                return defaultValue;
            }

            var headerBytes = headerValue as byte[];

            if (headerBytes == null)
            {
                return defaultValue;
            }

            return Encoding.UTF8.GetString(headerBytes);
        }
    }
}
