using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace SimpleCQRS.Contracts
{
    [ProtoContract]
    public class Envelope<T>
    {
        [ProtoMember(1)]
        public T Message { get; set; }

        [ProtoIgnore]
        public string MessageType { get { return typeof(T).FullName; } }
        
        [ProtoMember(2)]
        public string MessageId { get; set; }
        
        [ProtoIgnore]
        public string ReplyTo { get; set; }
    }
}
