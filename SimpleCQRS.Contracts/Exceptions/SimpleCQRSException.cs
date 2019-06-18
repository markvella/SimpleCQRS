using System;
using System.Runtime.Serialization;

namespace SimpleCQRS.Contracts.Exceptions
{
    [Serializable]
    public class SimpleCQRSException : Exception
    {        
        /// <inheritdoc />
        public SimpleCQRSException(string message) : base(message)
        {
        }
        
        /// <inheritdoc />
        public SimpleCQRSException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
        /// <inheritdoc />
        protected SimpleCQRSException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}