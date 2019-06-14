using System.IO;

namespace SimpleCQRS.Serializers.Protobuf
{
    public class ProtobufSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            using (var memStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memStream,obj);
                return memStream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            using (var memStream = new MemoryStream(data))
            {
                return ProtoBuf.Serializer.Deserialize<T>(memStream);
            }
        }
    }
}
