using System;

namespace SimpleCQRS.Serializers
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);

        object Deserialize(byte[] data, Type targetType);
    }
}