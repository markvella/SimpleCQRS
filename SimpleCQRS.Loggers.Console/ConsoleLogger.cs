using System;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace SimpleCQRS.Loggers.Console
{
    public class ConsoleLogger : ILogger
    {
        private readonly Logger _logger;
        private bool _disposed = false;

        public ConsoleLogger()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }

        public void Log(LogLevel logLevel, string message)
        {
            Log(logLevel, message, null);
        }

        public void Log(LogLevel logLevel, string message, Exception exception)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ConsoleLogger));
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                    _logger.Verbose(exception, message);
                    break;

                case LogLevel.Debug:
                    _logger.Debug(exception, message);
                    break;

                case LogLevel.Information:
                    _logger.Information(exception, message);
                    break;

                case LogLevel.Warning:
                    _logger.Warning(exception, message);
                    break;

                case LogLevel.Error:
                    _logger.Error(exception, message);
                    break;

                case LogLevel.Critical:
                    _logger.Fatal(exception, message);
                    break;
            }
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
                _logger?.Dispose();
            }

            _disposed = true;
        }
    }
}
