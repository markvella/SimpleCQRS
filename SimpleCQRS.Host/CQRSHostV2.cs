using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SimpleCQRS.Host.Configuration;
using SimpleCQRS.Host.Extensions;

namespace SimpleCQRS.Host
{
    public class CQRSHostV2 : IHost
    {
        private readonly HostConfiguration _config;
        private IConnection _connection;
        private readonly List<HostOperation> _operations;

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private bool _isStarted = false;
        private bool _disposed = false;

        internal CQRSHostV2(HostConfiguration config)
        {
            _config = config;
            _operations = new List<HostOperation>();
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
                DisposeOperations();
                DisposeConnection();
                _lock.Dispose();
            }

            _disposed = true;
        }

        private IList<OperationConfiguration> Operations => _config?.Operations;

        private string ExchangeName => $"{_config.ServiceName}.service.exchange";

        public async Task StartAsync()
        {
            await _lock?.WaitAsync();

            try
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(CQRSHostV2));
                }

                if (_isStarted)
                {
                    throw new Exception("Host already started.");
                }

                // Dispose the previous connection and operations (if any)
                DisposeOperations();
                DisposeConnection();

                // Create a new RabbitMQ connection
                _connection = _config.CreateConnection();

                // Create the service exchange
                using (var model = _connection.CreateModel())
                {
                    model.ExchangeDeclare(ExchangeName, "direct", true, false);
                }

                // Create a model, queue, bindings and consumer for each operation
                foreach (var operation in _config.Operations)
                {
                    _operations.Add(new HostOperation(operation, _connection, ExchangeName));
                }

                _isStarted = true;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task StopAsync()
        {
            await _lock?.WaitAsync();

            try
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(CQRSHostV2));
                }

                if (!_isStarted)
                {
                    throw new Exception("Host not started.");
                }

                // Dispose the previous connection and models (if any)
                DisposeOperations();
                DisposeConnection();

                _isStarted = false;
            }
            finally
            {
                _lock.Release();
            }
        }

        private void DisposeConnection()
        {
            _connection?.Dispose();
        }

        private void DisposeOperations()
        {
            foreach (var operation in _operations)
            {
                operation.Dispose();
            }

            _operations.Clear();
        }
    }
}
