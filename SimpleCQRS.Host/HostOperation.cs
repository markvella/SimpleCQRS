using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Contracts;
using SimpleCQRS.Host.Configuration;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host
{
    public interface IHostOperation<TRequest, TResponse> : IDisposable
    {
        void SendReply(Envelope<TRequest> env, object reply);
    }
   
    internal sealed class HostOperation<TRequest, TResponse> : IHostOperation<TRequest, TResponse>
    {
        private readonly OperationConfiguration<TRequest, TResponse> _operation;
        private readonly IConnection _connection;
        private readonly IModel _model;
        private bool _disposed = false;
        private readonly object _lock = new object();

        internal HostOperation(
            OperationConfiguration<TRequest, TResponse> operation,
            ISerializer serializer,
            IConnection connection,
            string exchangeName,
            string serviceName)
        {
            ServiceName = serviceName;
            ExchangeName = exchangeName;
            Serializer = serializer;

            _operation = operation;
            _connection = connection;
            _model = _connection.CreateModel();

            var queueName = QueueName;
            _model.QueueDeclare(queueName, true, false, false);
            _model.QueueBind(queueName, ExchangeName, operation.OperationName);

            var consumer = new EventingBasicConsumer(_model);
            _model.BasicConsume(queueName, false, consumer);
            consumer.Received += OnMessageReceived;
        }
        
        private ISerializer Serializer { get; }

        private Type MessageType => _operation.RequestType;

        private string ServiceName { get; }
        
        private string ExchangeName { get; }

        private string QueueName => $"{ServiceName}.{_operation.OperationName}.operation.queue";

        public void SendReply(Envelope<TRequest> env, object reply)
        {
            throw new NotImplementedException();
        }
        
        private void OnMessageReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var message = Serializer.Deserialize(e.Body, MessageType);
                //var envelope = message.Wrap();
                
                lock (_lock)
                {
                    //MessageHandler(obj, this);

                    // Acknowledge the message
                    _model.BasicAck(e.DeliveryTag, false);
                }
            }
            catch (Exception exception)
            {
                lock (_lock)
                {
                    // Acknowledge the message
                    _model.BasicNack(e.DeliveryTag, false, true);
                }

                throw;
            }
            
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
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                lock (_lock)
                {
                    _model?.Dispose();
                    _connection?.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
