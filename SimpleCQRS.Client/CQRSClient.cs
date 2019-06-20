using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SimpleCQRS.Client.Configuration;
using SimpleCQRS.Client.Extensions;
using SimpleCQRS.Loggers;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Client
{
    public sealed class CQRSClient<TRequest, TResponse> : IClient<TRequest, TResponse> 
    {
        private readonly ClientConfiguration _config;
        private readonly IConnection _connection;
        private readonly IModel[] _publisherModels;
        private readonly object[] _publisherLocks;
        private long _publisherCurrentIndex = -1;

        private readonly AsyncMessageConsumer[] _consumers;
        private long _consumerCurrentIndex = -1;
        private bool _disposed = false;
        
        internal CQRSClient(ClientConfiguration config)
        {
            _config = config;
            _connection = config.CreateConnection();
            
            _publisherModels = new IModel[PublishingPoolSize];
            _publisherLocks = new object[PublishingPoolSize];
            
            for (var i = 0; i < PublishingPoolSize; i++)
            {
                _publisherModels[i] = _connection.CreateModel();
                _publisherLocks[i] = new object();
            }
            
            _consumers = new AsyncMessageConsumer[ConsumingPoolSize];
            
            for (var i = 0; i < ConsumingPoolSize; i++)
            {
                var consumerModel = _connection.CreateModel();
                var consumerQueueName = $"req_{Guid.NewGuid().ToString()}";

                consumerModel.QueueDeclare(consumerQueueName, false, true, true, new Dictionary<string, object>());
                var consumer = _consumers[i] = new AsyncMessageConsumer(consumerModel, Logger, consumerQueueName);
                consumerModel.BasicConsume(consumerQueueName, true, consumer);
            }
        }

        private string ExchangeName => $"{_config.ServiceName}.service.exchange";
        private int PublishingPoolSize => _config.PublishingPoolSize;
        private int ConsumingPoolSize => _config.ConsumingPoolSize;
        private string ServiceName => _config.ServiceName;
        private string OperationName => _config.OperationName;
        private TimeSpan MaximumTimeout => _config.MaximumTimeout;
        private ISerializer Serializer => _config?.Serializer;
        private ILogger Logger => _config?.Logger;
        
        public async Task<TResponse> RequestAsync(TRequest request, CancellationToken ct)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            try
            {
                var requestId = Guid.NewGuid().ToString();
                var requestData = Serializer.Serialize(request);
            
                var publisherIndex = GetNextPublisherIndex(PublishingPoolSize);
                var publisherModel = _publisherModels[publisherIndex];
                var publisherLock = _publisherLocks[publisherIndex];

                var consumerIndex = GetNextConsumerIndex(ConsumingPoolSize);
                var consumer = _consumers[consumerIndex];

                lock (publisherLock)
                {
                    var props = publisherModel.CreateBasicProperties();
                    props.Persistent = false;
                    props.CorrelationId = requestId;
                    props.ReplyTo = consumer.QueueName;

                    consumer.AddRequest(requestId);
             
                    publisherModel.BasicPublish(ExchangeName, OperationName, props, requestData);
                }

                var responseData = await consumer.GetResponse(requestId, MaximumTimeout, ct);
                var response = Serializer.Deserialize<TResponse>(responseData);
                
                return response;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "An unhandled exception occured.", ex);
                
                throw;
            }
        }

        private int GetNextPublisherIndex(int poolSize)
        {
            var index = Interlocked.Increment(ref _publisherCurrentIndex) % poolSize;
            return Convert.ToInt32(index);
        }
        
        private int GetNextConsumerIndex(int poolSize)
        {
            var index = Interlocked.Increment(ref _consumerCurrentIndex) % poolSize;
            return Convert.ToInt32(index);
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
                _connection?.Dispose();
                _consumers?.ToList().ForEach(c => c.Dispose());
                _publisherModels?.ToList().ForEach(p => p.Dispose());
            }

            _disposed = true;
        }
    }
}