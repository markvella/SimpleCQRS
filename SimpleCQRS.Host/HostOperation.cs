using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Host.Configuration;
using SimpleCQRS.Host.Extensions;

namespace SimpleCQRS.Host
{
    public interface IHostOperation : IDisposable
    {

    }

    internal class HostOperation : IHostOperation
    {
        private readonly OperationConfiguration _operation;
        private readonly IConnection _connection;
        private readonly IModel _model;
        private bool _disposed = false;
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        internal HostOperation(OperationConfiguration operation, IConnection connection, string exchangeName)
        {
            _operation = operation;
            _connection = connection;
            ExchangeName = exchangeName;
            _model = _connection.CreateModel();

            var queueName = QueueName;
            _model.QueueDeclare(queueName, true, false, false);
            _model.QueueBind(queueName, ExchangeName, operation.OperationName);

            var consumer = new EventingBasicConsumer(_model);
            _model.BasicConsume(queueName, false, consumer);
            consumer.Received += Consumer_Received;
        }

        private string ExchangeName { get; }

        private string QueueName => $"{_operation.OperationName}.operation.queue";

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            //Console.WriteLine("Message received");

            var typeName = e.GetHeaderValue("type");

            if (typeName.Equals("ping"))
            {
                return;
            }

            var type = Type.GetType(typeName);
            //var model = _models[type];
            //var responseQ = Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["responsequeue"]);
            //var requestId = Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["requestId"]);
            //var service = (BaseRequestHandler)_serviceProvider.GetService(_handlers[type]);
            //var memStream = new MemoryStream(e.Body);

            //var envelope = ProtoBuf.Serializer.Deserialize(typeof(Envelope<>).MakeGenericType(type), memStream);
            //var result = service.Process(envelope);
            //var message = result.GetAwaiter().GetResult();

            //var props = model.CreateBasicProperties();
            //Dictionary<string, object> headers = new Dictionary<string, object>();
            //headers.Add("requestId", requestId);
            //props.Headers = headers;
            //memStream = new MemoryStream();
            //ProtoBuf.Serializer.Serialize(memStream, message);
            //model.BasicPublish("", responseQ, props, memStream.ToArray());
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _model?.Dispose();
                _connection?.Dispose();
                _lock.Dispose();
            }

            _disposed = true;
        }
    }
}
