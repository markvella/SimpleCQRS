using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCQRS.Contracts
{
    public class Envelope<T>
    {
        public T Payload { get; set; }
        public string MessageType { get { return typeof(T).FullName; } }
        public string MessageId { get; set; }
    }
}
