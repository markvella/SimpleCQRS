using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using SimpleCQRS.Contracts;

namespace SimpleCQRS.Serializers.Protobuf
{
    public class ProtobufSerializer:ISerializer
    {
        public async Task<T> Deserialize<T>(byte[] content)
        {
            using (var memStream = new MemoryStream(content))
            {
                return ProtoBuf.Serializer.Deserialize<T>(memStream);
            }
        }

        public async Task<object> Deserialize(Type tp, byte[] content)
        {
            using (var memStream = new MemoryStream(content))
            {
                return ProtoBuf.Serializer.Deserialize(tp,memStream);
            }
        }

        public async Task<byte[]> Serialize<T>(T obj)
        {
            using (var memStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memStream,obj);
                return memStream.ToArray();
            }
        }
    }
}
