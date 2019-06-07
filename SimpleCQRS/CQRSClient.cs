using Newtonsoft.Json;
using ProtoBuf;
using RabbitMQ.Client;
using SimpleCQRS.Contracts;
using System;
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
        private readonly int _poolSize = 8;
        private int currentModelIdx = 0;
        private object indexLock;
        private readonly CustomConsumer _consumer;
        private string _responseQueueName;

        public CQRSClient()
        {
            RabbitMQ.Client.ConnectionFactory factory = new RabbitMQ.Client.ConnectionFactory();
            _connection = factory.CreateConnection();
            _listenerModel = _connection.CreateModel();

            _publishers = new IModel[_poolSize];
            _locks = new object[_poolSize];
            for (int i=0;i<_poolSize;i++)
            {
                _publishers[i] = _connection.CreateModel();
                _locks[i] = new object();
            }
            indexLock = new object();
            _consumer = new CustomConsumer(_listenerModel);
            _responseQueueName = $"req_{Guid.NewGuid().ToString()}";
            DeclareQueue(_responseQueueName).GetAwaiter().GetResult();
            _listenerModel.BasicConsume(_responseQueueName, true, _consumer);
            //_model.BasicQos(0, 1, true);
        }

        public void Dispose()
        {
            _connection.Dispose();
            _listenerModel.Dispose();
        }

        private int GetNextModelIdx()
        {
            lock (indexLock)
            {
                var returnIdx = currentModelIdx++;
                if (returnIdx >= _poolSize)
                {
                    returnIdx = 0;
                }
                return returnIdx;
            }
        }

        public async Task<TResponse> Request<T, TResponse>(T Request)
        {
            var requestType = typeof(T);
            var exchangeName = $"ex_{requestType.FullName}";
            var requestEnvelope = new Envelope<T> { Payload = Request, MessageId = Guid.NewGuid().ToString() };

            var props = _listenerModel.CreateBasicProperties();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            dictionary.Add("type", requestType.AssemblyQualifiedName);
            dictionary.Add("responsequeue", _responseQueueName);
            dictionary.Add("requestId", requestEnvelope.MessageId);
            props.Headers = dictionary;
            var req = Serialize(requestEnvelope);
            var requestPublisherIdx = GetNextModelIdx();
            _consumer.AddRequest(requestEnvelope.MessageId);
            lock (_locks[requestPublisherIdx])
            {
                _publishers[requestPublisherIdx].BasicPublish(exchangeName, "", props, req);
            }

            var ary = await _consumer.GetResponse(requestEnvelope.MessageId);
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

        private async Task<BasicGetResult> GetResult(IModel model, string queue)
        {
            BasicGetResult result = null;
            while (result == null)
            {
                result = model.BasicGet(queue, true);
            }
            return result;
        }
    }

    public class ResponseObject
    {
        public SemaphoreSlim Semaphore { get; set; }
        public byte[] Response { get; set; }
    }

    public class CustomConsumer : DefaultBasicConsumer
    {
        private Dictionary<string, ResponseObject> responses = new Dictionary<string,ResponseObject>();
        public void AddRequest(string requestId)
        {
            responses.Add(requestId, new ResponseObject { Semaphore = new SemaphoreSlim(0, 1), Response = null });
        }

        public async Task<byte[]> GetResponse(string requestId)
        {
            await responses[requestId].Semaphore.WaitAsync();
            return responses[requestId].Response;
        }

        public CustomConsumer(IModel model):base(model)
        {
        }
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            //base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var requestId = Encoding.UTF8.GetString((byte[])properties.Headers["requestId"]);
            responses[requestId].Response = body;
            responses[requestId].Semaphore.Release();
        }
    }
}
