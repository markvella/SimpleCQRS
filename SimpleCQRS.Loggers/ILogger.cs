using System;

namespace SimpleCQRS.Loggers
{
    /// <summary>
    /// Represents a type used to perform logging.
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="message">The message that describes the log entry.</param>
        /// <param name="exception">The exception related to this entry.</param>
        void Log(LogLevel logLevel, string message, Exception exception);
    }
}
