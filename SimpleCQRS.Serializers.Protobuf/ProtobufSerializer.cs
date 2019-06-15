using System.IO;
using ProtoBuf;

namespace SimpleCQRS.Serializers.Protobuf
{
    public class ProtobufSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            using (var memStream = new MemoryStream())
            {
                Serializer.Serialize(memStream,obj);
                return memStream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            using (var memStream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(memStream);
            }
        }
    }
}
