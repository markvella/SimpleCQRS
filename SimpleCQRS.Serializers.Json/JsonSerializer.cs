using System.Text;
using Newtonsoft.Json;

namespace SimpleCQRS.Serializers.Json
{
    public class JsonSerializer : ISerializer
    {
        private readonly Encoding _encoder = Encoding.UTF8;
        private readonly JsonSerializerSettings _settings;
        
        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }
        
        public byte[] Serialize<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.None, _settings);
            return _encoder.GetBytes(json);
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null)
                return default;
            
            var json = _encoder.GetString(data);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}