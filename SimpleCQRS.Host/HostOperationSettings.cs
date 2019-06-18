using RabbitMQ.Client;
using SimpleCQRS.Loggers;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host
{
    internal sealed class HostOperationSettings
    {
        internal IConnection Connection { get; set; }
        internal ISerializer Serializer { get; set; }
        internal ILogger Logger { get; set; }
        internal string ServiceName { get; set; }
        internal string ExchangeName { get; set; }
    }
}
