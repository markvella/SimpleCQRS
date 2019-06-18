using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace SimpleCQRS.Client
{
    internal class CustomConsumer : DefaultBasicConsumer, IDisposable
    {
        private bool _disposed = false;
        private ConcurrentDictionary<string, ResponseObject> _responses = new ConcurrentDictionary<string,ResponseObject>();

        internal CustomConsumer(IModel model, string queueName = null) : base(model)
        {
            Model = model;
            QueueName = queueName;
        }
        
        public string QueueName { get; }
        
        public void AddRequest(string requestId)
        {
            _responses.TryAdd(requestId, new ResponseObject
            {
                Semaphore = new SemaphoreSlim(0, 1),
                Response = null
            });
        }

        public Task<byte[]> GetResponse(string requestId)
        {
            return GetResponse(requestId, TimeSpan.MaxValue, CancellationToken.None);
        }
        
        public async Task<byte[]> GetResponse(string requestId, TimeSpan maxTimeout, CancellationToken ct)
        {
            var response = _responses[requestId];
            await response.Semaphore.WaitAsync(maxTimeout, ct);
            return response.Response;
        }
        
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var requestId = properties.CorrelationId;
            var response = _responses[requestId];
            response.Response = body;
            response.Semaphore.Release();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Model?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}