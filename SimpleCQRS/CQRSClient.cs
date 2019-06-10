using Newtonsoft.Json;
using ProtoBuf;
using RabbitMQ.Client;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleCQRS
{
    public class CQRSClient : ICQRSClient, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _listenerModel;
        private readonly IModel[] _publishers;
        private readonly object[] _locks;
        private readonly int _outPoolSize = 8;
        private readonly int _inPoolSize = 1;
        private int currentModelIdx = 0;
        private object indexLock;
        //private readonly CustomConsumer _consumer;
        private readonly CustomConsumer[] _consumers;
        private string[] _responseQueueName;

        public CQRSClient()
        {
            RabbitMQ.Client.ConnectionFactory factory = new RabbitMQ.Client.ConnectionFactory();
            _connection = factory.CreateConnection();
            _listenerModel = _connection.CreateModel();

            _publishers = new IModel[_outPoolSize];
            _locks = new object[_outPoolSize];
            for (int i=0;i<_outPoolSize;i++)
            {
                _publishers[i] = _connection.CreateModel();
                _locks[i] = new object();
            }
            _consumers = new CustomConsumer[_inPoolSize];
            _responseQueueName = new string[_inPoolSize];
            for (int i=0;i<_inPoolSize; i++)
            {
                _responseQueueName[i] = $"req_{Guid.NewGuid().ToString()}";
                DeclareQueue(_responseQueueName[i]).GetAwaiter().GetResult();
                _consumers[i] = new CustomConsumer(_listenerModel);
                _listenerModel.BasicConsume(_responseQueueName[i], true, _consumers[i]);
            }
            //_model.BasicQos(0, 1, true);
        }

        public void Dispose()
        {
            _connection.Dispose();
            _listenerModel.Dispose();
        }

        private int GetNextModelIdx()
        {
            return currentModelIdx++ % _outPoolSize;            
        }

        public async Task<TResponse> Request<T, TResponse>(T Request)
        {
            var requestType = typeof(T);
            var exchangeName = $"ex_{requestType.FullName}";
            var requestEnvelope = new Envelope<T> { Message = Request, MessageId = Guid.NewGuid().ToString() };
            var requestPublisherIdx = GetNextModelIdx();

            var props = _listenerModel.CreateBasicProperties();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            dictionary.Add("type", requestType.AssemblyQualifiedName);
            dictionary.Add("responsequeue", _responseQueueName[requestPublisherIdx%_inPoolSize]);
            dictionary.Add("requestId", requestEnvelope.MessageId);
            props.Headers = dictionary;
            var req = Serialize(requestEnvelope);
            _consumers[requestPublisherIdx%_inPoolSize].AddRequest(requestEnvelope.MessageId);
            lock (_locks[requestPublisherIdx])
            {
                _publishers[requestPublisherIdx].BasicPublish(exchangeName, "", props, req);
            }

            var ary = await _consumers[requestPublisherIdx % _inPoolSize].GetResponse(requestEnvelope.MessageId);
            var memStream = new MemoryStream(ary);
            var response = ProtoBuf.Serializer.Deserialize<TResponse>(memStream);
            return response;

        }

        private byte[] Serialize<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj); // or SerializeWithLengthPrefix
                return stream.ToArray();
            }
        }

        private async Task DeclareQueue(string responseQueueName)
        {
            _listenerModel.QueueDeclareNoWait(responseQueueName, false, true, true, new Dictionary<string, object>());
        }
    }

    public class ResponseObject
    {
        public SemaphoreSlim Semaphore { get; set; }
        public byte[] Response { get; set; }
    }

    public class CustomConsumer : DefaultBasicConsumer
    {
        private ConcurrentDictionary<string, ResponseObject> responses = new ConcurrentDictionary<string,ResponseObject>();
        public void AddRequest(string requestId)
        {
            responses.TryAdd(requestId, new ResponseObject { Semaphore = new SemaphoreSlim(0, 1), Response = null });
        }

        public async Task<byte[]> GetResponse(string requestId)
        {
            var response = responses[requestId];
            await response.Semaphore.WaitAsync();
            return response.Response;
        }

        public CustomConsumer(IModel model):base(model)
        {
        }
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var requestId = Encoding.UTF8.GetString((byte[])properties.Headers["requestId"]);
            var response = responses[requestId];
            response.Response = body;
            response.Semaphore.Release();
        }
    }
}
