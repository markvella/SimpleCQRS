using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace SimpleCQRS.Host
{
    public class CQRSHost : ICQRSHost
    {
        private readonly Dictionary<Type, Type> _handlers;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private Dictionary<Type, IModel> _models = new Dictionary<Type, IModel>();
        private IModel _model;

        public CQRSHost(Dictionary<Type, Type> handlers, IServiceProvider serviceProvider)
        {
            _handlers = handlers;
            _serviceProvider = serviceProvider;
            _connection = new ConnectionFactory().CreateConnection();
        }

        public async Task StartAsync()
        {
            foreach (var handler in _handlers)
            {
                var currentModel = _models[handler.Key] = _connection.CreateModel();
                var queueResponse = currentModel.QueueDeclare($"q_{handler.Key.FullName}", false, false, true);
                currentModel.ExchangeDeclare($"ex_{handler.Key.FullName}", "fanout", true, false);
                currentModel.QueueBind($"q_{handler.Key.FullName}", $"ex_{handler.Key.FullName}", "");
                var consumer = new EventingBasicConsumer(currentModel);
                currentModel.BasicConsume($"q_{handler.Key.FullName}", true, consumer);

                consumer.Received += Consumer_Received;
            }
            Console.ReadLine();


        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            //Console.WriteLine("Message received");
            if (Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["type"]) == "ping")
                return;
            var type = Type.GetType(Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["type"]));
            var model = _models[type];
            var responseQ = Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["responsequeue"]);
            var requestId = Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["requestId"]);
            var service = (BaseRequestHandler)_serviceProvider.GetService(_handlers[type]);
            var memStream = new MemoryStream(e.Body);
            
            var envelope = ProtoBuf.Serializer.Deserialize(typeof(Envelope<>).MakeGenericType(type), memStream);
            var result = service.Process(envelope);
            var message = result.GetAwaiter().GetResult();

            var props = model.CreateBasicProperties();
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("requestId", requestId);
            props.Headers = headers;
            memStream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(memStream, message);
            model.BasicPublish("", responseQ, props, memStream.ToArray());

        }
    }
}
