using ProtoBuf;

namespace Sample.Contracts
{
    [ProtoContract]
    public class HelloWorldResponse
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
