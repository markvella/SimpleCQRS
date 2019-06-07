using ProtoBuf;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    [ProtoContract]
    public class HelloWorldResponse
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
