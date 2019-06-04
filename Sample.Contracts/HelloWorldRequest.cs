using ProtoBuf;
using SimpleCQRS.Contracts;
using System;

namespace Sample.Contracts
{
    [ProtoContract]
    public class HelloWorldRequest
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
