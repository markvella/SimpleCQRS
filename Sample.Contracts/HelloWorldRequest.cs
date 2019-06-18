using ProtoBuf;

namespace Sample.Contracts
{
    [ProtoContract]
    public class HelloWorldRequest
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
