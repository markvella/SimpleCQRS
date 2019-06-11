using System;
using System.IO;
using Newtonsoft.Json;

namespace SimpleCQRS.Serializers.Json
{
    public class JsonSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public JsonSerializer()
        {
            _serializer = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }
        
        public object Deserialize(byte[] data, Type targetType)
        {
            using (var ms = new MemoryStream(data))
            using (var sr = new StreamReader(ms))
            using (var reader = new JsonTextReader(sr))
            {
                return _serializer.Deserialize(reader, targetType);
            }
        }

        public byte[] Serialize<T>(T obj)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            using (var writer = new JsonTextWriter(sw))
            {
                _serializer.Serialize(writer, obj);
                return ms.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var sr = new StreamReader(ms))
            using (var reader = new JsonTextReader(sr))
            {
                return _serializer.Deserialize<T>(reader);
            }
        }
    }
}