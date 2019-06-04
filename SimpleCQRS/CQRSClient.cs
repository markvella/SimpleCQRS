using Newtonsoft.Json;
using RabbitMQ.Client;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;
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
            _model.BasicPublish(exchangeName, "", props, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestEnvelope)));
            var resultTask = GetResult(_model, responseQueueName);
            CancellationToken token = new CancellationToken();
            BasicGetResult result = null;
            if (await Task.WhenAny(resultTask, Task.Delay(3000, token)) == resultTask)
            {
                result = await resultTask;

            }
            else
            {
                throw new TimeoutException();
            }
            _model.QueueDeleteNoWait(responseQueueName,false,false);
            return JsonConvert.DeserializeObject<TResponse>(Encoding.UTF8.GetString(result.Body));

        }
        private async Task DeclareQueue(string responseQueueName)
        {
            _model.QueueDeclareNoWait(responseQueueName, false, false, true, new Dictionary<string,object>());
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
}
