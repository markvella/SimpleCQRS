using System;

namespace SimpleCQRS.Serializers
{
    public class NullSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            throw new NotSupportedException();
        }

        public T Deserialize<T>(byte[] data)
        {
            throw new NotSupportedException();
        }
    }
}