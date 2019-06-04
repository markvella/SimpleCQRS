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
        private readonly IModel _model;

        public CQRSClient()
        {
            RabbitMQ.Client.ConnectionFactory factory = new RabbitMQ.Client.ConnectionFactory();
            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();
        }

        public void Dispose()
        {
            _connection.Dispose();
            _model.Dispose();
        }

        public async Task<TResponse> Request<T, TResponse>(T Request)
        {
            var requestType = typeof(T);
            var exchangeName = $"ex_{requestType.FullName}";
            var requestEnvelope = new Envelope<T> { Payload = Request, MessageId = Guid.NewGuid().ToString() };
            var responseQueueName = $"req_{requestType.FullName}_{requestEnvelope.MessageId}";

            var props = _model.CreateBasicProperties();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            dictionary.Add("type", requestType.AssemblyQualifiedName);
            dictionary.Add("responsequeue", responseQueueName);
            props.Headers = dictionary;
            await DeclareQueue(responseQueueName);
            SemaphoreSlim slimLock = new SemaphoreSlim(1, 1);
            byte[] outAry = new byte[0];
            var consumer = new CustomConsumer(outAry, slimLock);
            _model.BasicConsume(responseQueueName, true, consumer);
            var req = Serialize(requestEnvelope);
            _model.BasicPublish(exchangeName, "", props, req);
            await slimLock.WaitAsync();

            var memStream = new MemoryStream(outAry);
            var response = ProtoBuf.Serializer.Deserialize<TResponse>(memStream);
            _model.QueueDeleteNoWait(responseQueueName, false, false);
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
            _model.QueueDeclareNoWait(responseQueueName, false, false, true, new Dictionary<string, object>());
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

    public class CustomConsumer : DefaultBasicConsumer
    {
        private byte[] _context;
        private SemaphoreSlim _slimLock;

        public CustomConsumer(byte[] context, SemaphoreSlim slimLock)
        {
            _context = context;
            _slimLock = slimLock;
        }
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            _context = body;
            _slimLock.Release();
        }
    }
}
