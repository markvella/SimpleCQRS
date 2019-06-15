using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Contracts;
using SimpleCQRS.Host.Configuration;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host
{
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

        private Action<Envelope<TRequest>, IHostOperation<TRequest, TResponse>> MessageHandler => _operation.Handler;

        private string ServiceName { get; }
        
        private string ExchangeName { get; }

        private string OperationName => _operation.OperationName;
        
        private string QueueName => $"{ServiceName}.{OperationName}.operation.queue";

        public void SendReply(Envelope<TRequest> env, TResponse reply)
        {
            lock (_lock)
            {
                var props = _model.CreateBasicProperties();
                props.CorrelationId = env.MessageId;
                
                var replyData = Serializer.Serialize(reply);
                
                _model.BasicPublish(string.Empty, env.ReplyTo, props, replyData);
            }
        }
        
        private void OnMessageReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var envelope = ParseMessage(e);

                // Execute the message handler within the lock to ensure
                // the reply is safely done on the same IModel
                MessageHandler(envelope, this);
                
                lock (_lock)
                {
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

                // TODO: Add logging
                
                throw;
            }
        }

        private Envelope<TRequest> ParseMessage(BasicDeliverEventArgs e)
        {
            var message = Serializer.Deserialize<TRequest>(e.Body);
            
            var envelope = new Envelope<TRequest>
            {
                Message = message,
                MessageId = e.BasicProperties.CorrelationId,
                ReplyTo = e.BasicProperties.ReplyTo,
                RoutingKey = e.RoutingKey
            };

            return envelope;
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
