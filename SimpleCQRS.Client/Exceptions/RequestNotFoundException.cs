using System;
using System.Runtime.Serialization;
using SimpleCQRS.Contracts.Exceptions;

namespace SimpleCQRS.Client.Exceptions
{
    [Serializable]
    public class RequestNotFoundException : SimpleCQRSException
    {        
        /// <inheritdoc />
        public RequestNotFoundException(string message) : base(message)
        {
        }
        
        /// <inheritdoc />
        protected RequestNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}