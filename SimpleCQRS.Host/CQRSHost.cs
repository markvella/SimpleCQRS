using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SimpleCQRS.Host.Configuration;
using SimpleCQRS.Host.Extensions;
using SimpleCQRS.Loggers;
using SimpleCQRS.Serializers;

namespace SimpleCQRS.Host
{
    public class CQRSHost : IHost
    {
        private readonly HostConfiguration _config;
        private IConnection _connection;
        private readonly List<IDisposable> _operations;

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private bool _isStarted = false;
        private bool _disposed = false;

        internal CQRSHost(HostConfiguration config)
        {
            _config = config;
            _operations = new List<IDisposable>();
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

        private ISerializer Serializer => _config?.Serializer;

        private ILogger Logger => _config?.Logger;

        private string ServiceName => _config?.ServiceName;
        
        private string ExchangeName => $"{_config.ServiceName}.service.exchange";

        public async Task StartAsync()
        {
            Logger.Log(LogLevel.Trace, "Starting CQRS host");

            await _lock?.WaitAsync();

            try
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(CQRSHost));
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
                var hostOperationGenericType = typeof(HostOperation<,>);

                var hostOperationSettings = new HostOperationSettings()
                {
                    Connection = _connection,
                    Serializer = Serializer,
                    Logger = Logger,
                    ServiceName = ServiceName,
                    ExchangeName = ExchangeName
                };

                foreach (var operation in _config.Operations)
                {
                    Type[] hostOperationTypeArgs = { operation.RequestType, operation.ResponseType };
                    var hostOperationType = hostOperationGenericType.MakeGenericType(hostOperationTypeArgs);

                    object[] hostOperationParams = {operation, hostOperationSettings };

                    var hostOperation = (IDisposable) Activator.CreateInstance(
                        hostOperationType,
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        hostOperationParams,
                        CultureInfo.InvariantCulture);
                    
                    _operations.Add(hostOperation);
                }

                Logger.Log(LogLevel.Trace, "Started CQRS host");

                _isStarted = true;
            }
            finally
            {
                _lock?.Release();
            }
        }

        public async Task StopAsync()
        {
            Logger.Log(LogLevel.Trace, "Stopping CQRS host");

            await _lock?.WaitAsync();

            try
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(CQRSHost));
                }

                if (!_isStarted)
                {
                    throw new Exception("Host not started.");
                }

                // Dispose the previous connection and models (if any)
                DisposeOperations();
                DisposeConnection();

                Logger.Log(LogLevel.Trace, "Stopped CQRS host");

                _isStarted = false;
            }
            finally
            {
                _lock?.Release();
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
