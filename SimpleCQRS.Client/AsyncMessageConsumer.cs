using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Contracts.Exceptions;

namespace SimpleCQRS.Client
{
    internal class AsyncMessageConsumer : AsyncEventingBasicConsumer, IDisposable
    {
        private bool _disposed = false;
        private ConcurrentDictionary<string, ResponseObject> _responses = new ConcurrentDictionary<string,ResponseObject>();

        internal AsyncMessageConsumer(IModel model, string queueName = null) : base(model)
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
            _responses.TryRemove(requestId, out var response);

            if (response == null)
            {
                throw new SimpleCQRSException("Request not found.");
            }

            var waitResult = await response.Semaphore.WaitAsync(maxTimeout, ct);

            if (ct.IsCancellationRequested)
            {
                throw new SimpleCQRSException("Request operation cancelled.");
            }
        
            if (!waitResult)
            {
                throw new SimpleCQRSException($"Response took longer than {maxTimeout} to respond.");    
            }
        
            return response.Response;
        }
        
        public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            await base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var requestId = properties.CorrelationId;

            _responses.TryGetValue(requestId, out var response);

            if (response == null)
            {
                return;
            }
            
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