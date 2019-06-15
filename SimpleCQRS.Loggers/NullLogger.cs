using System;

namespace SimpleCQRS.Loggers
{
    public class NullLogger : ILogger
    {
        public void Log(LogLevel logLevel, string message, Exception exception)
        {
            // Do nothing
        }

        public void Dispose()
        {
            // Do nothing
        }
    }
}
