using System;
using System.Runtime.Serialization;
using SimpleCQRS.Contracts.Exceptions;

namespace SimpleCQRS.Client.Exceptions
{
    [Serializable]
    public class OperationTimeoutException : SimpleCQRSException
    {        
        /// <inheritdoc />
        public OperationTimeoutException(string message) : base(message)
        {
        }
        
        /// <inheritdoc />
        protected OperationTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}