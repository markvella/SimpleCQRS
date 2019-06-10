using System;

namespace SimpleCQRS.Serializers
{
    public class NullSerializer : ISerializer
    {
        public byte[] Serialize(object obj)
        {
            throw new NotSupportedException();
        }

        public object Deserialize(byte[] data, Type targetType)
        {
            throw new NotSupportedException();
        }
    }
}