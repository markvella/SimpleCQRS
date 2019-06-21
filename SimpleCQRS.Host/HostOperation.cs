using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Contracts;
using SimpleCQRS.Host.Configuration;
using SimpleCQRS.Loggers;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host
{
    internal sealed class HostOperation<TRequest, TResponse> : IHostOperation<TRequest, TResponse>
    {
        private readonly OperationConfiguration<TRequest, TResponse> _operation;
        private readonly IConnection _connection;
        private bool _disposed = false;
        private readonly Dictionary<string, HostModel> _models;
        
        internal HostOperation(
            OperationConfiguration<TRequest, TResponse> operation,
            HostOperationSettings settings)
        {
            ServiceName = settings.ServiceName;
            ExchangeName = settings.ExchangeName;
            Serializer = settings.Serializer;
            Logger = settings.Logger;

            _operation = operation;
            _connection = settings.Connection;
            _models = new Dictionary<string, HostModel>();

            CreateQueue();

            for (var i = 0; i < NumberOfConsumers; i++)
            {
                var model = _connection.CreateModel();
            
                var consumer = new AsyncEventingBasicConsumer(model);
                var consumerTag = model.BasicConsume(QueueName, true, consumer);
                consumer.Received += OnMessageReceived;
                
                _models.Add(consumerTag, new HostModel(model, consumerTag));
            }
        }

        private class HostModel
        {
            internal HostModel(IModel model, string consumerTag)
            {
                Model = model;
                ConsumerTag = consumerTag;
                Lock = new object();
            }

            internal IModel Model { get; }
            internal object Lock { get; }
            internal string ConsumerTag { get; }
        }
        
        private ISerializer Serializer { get; }

        private ILogger Logger { get; }

        private Func<Envelope<TRequest>, IHostOperation<TRequest, TResponse>, Task> MessageHandler => _operation.Handler;

        private string ServiceName { get; }
        
        private string ExchangeName { get; }

        private string OperationName => _operation.OperationName;
        
        private string QueueName => $"{ServiceName}.{OperationName}.operation.queue";

        private int NumberOfConsumers => _operation.NumberofOfConsumers;
        
        private void CreateQueue()
        {
            using (var model = _connection.CreateModel())
            {
                var queueOptionalArguments = new Dictionary<string, object>
                {
                    // Add the x-max-priority header to support message priority ordering
                    {
                        "x-max-priority", 10
                    }
                };
                
                model.QueueDeclare(QueueName, false, false, false, queueOptionalArguments);
                model.QueueBind(QueueName, ExchangeName, OperationName);
            }
        }
        
        public void SendReply(Envelope<TRequest> env, TResponse reply)
        {
            if (_models.TryGetValue(env.ConsumerTag, out var hostModel))
            {
                lock (hostModel.Lock)
                {
                    var props = hostModel.Model.CreateBasicProperties();
                    props.CorrelationId = env.MessageId;
                    props.Headers = env.Headers;
                
                    var replyData = Serializer.Serialize(reply);
                
                    hostModel.Model.BasicPublish(string.Empty, env.ReplyTo, props, replyData);
                }
            }
        }
        
        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var envelope = ParseMessage(e);

                // Execute the message handler
                await MessageHandler(envelope, this);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "An unhandled exception occured while handing message.", ex);
                
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
                RoutingKey = e.RoutingKey,
                ConsumerTag = e.ConsumerTag,
                Headers = e.BasicProperties?.Headers
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
                _models.Values.ToList().ForEach(m => m.Model?.Dispose());
                _connection?.Dispose();
            }

            _disposed = true;
        }
    }
}
