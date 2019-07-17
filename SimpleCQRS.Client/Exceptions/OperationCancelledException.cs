using System;
using System.Runtime.Serialization;
using SimpleCQRS.Contracts.Exceptions;

namespace SimpleCQRS.Client.Exceptions
{
    [Serializable]
    public class OperationCancelledException : SimpleCQRSException
    {        
        /// <inheritdoc />
        public OperationCancelledException(string message) : base(message)
        {
        }
        
        /// <inheritdoc />
        protected OperationCancelledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}