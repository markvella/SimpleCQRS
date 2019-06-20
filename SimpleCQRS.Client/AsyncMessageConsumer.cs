using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Client.Exceptions;
using SimpleCQRS.Contracts.Exceptions;
using SimpleCQRS.Loggers;

namespace SimpleCQRS.Client
{
    internal sealed class AsyncMessageConsumer : AsyncEventingBasicConsumer, IDisposable
    {
        private bool _disposed = false;
        private readonly ILogger Logger;
        private readonly ConcurrentDictionary<string, ResponseObject> _responses = new ConcurrentDictionary<string,ResponseObject>();

        internal AsyncMessageConsumer(IModel model, ILogger logger, string queueName) : base(model)
        {
            Model = model;
            Logger = logger;
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
        
        public async Task<byte[]> GetResponse(string requestId, TimeSpan maxTimeout, CancellationToken ct)
        {
            try
            {
                _responses.TryGetValue(requestId, out var response);

                if (response == null)
                {
                    throw new RequestNotFoundException($"Corresponding request with key {requestId} was not found.");
                }

                var waitResult = await response.Semaphore.WaitAsync(maxTimeout, ct);

                if (ct.IsCancellationRequested)
                {
                    throw new OperationCancelledException("Request operation cancelled.");
                }
        
                if (!waitResult)
                {
                    throw new OperationTimeoutException($"Response took longer than {maxTimeout} to respond.");    
                }
        
                return response.Response;
            }
            finally
            {
                _responses.TryRemove(requestId, out var response);
            }
        }
        
        public override Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            //await base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var requestId = properties.CorrelationId;

            _responses.TryGetValue(requestId, out var response);

            if (response == null)
            {
                Logger.Log(LogLevel.Warning, $"Request not found for key {requestId}");
            }
            else
            {
                response.Response = body;
                response.Semaphore.Release();
            }

            return Task.CompletedTask;
        }

        private void Dispose(bool disposing)
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