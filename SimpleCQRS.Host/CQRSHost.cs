using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Contracts;

namespace SimpleCQRS.Host
{
    public class CQRSHost : ICQRSHost
    {
        private readonly Dictionary<Type, Type> _handlers;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private Dictionary<Type, IModel> _models = new Dictionary<Type, IModel>();
        private IModel _model;
        private readonly string _serviceName;
        private readonly ISerializer _serializer;

        public CQRSHost(Dictionary<Type, Type> handlers, IServiceProvider serviceProvider, ISerializer serializer,string serviceName)
        {
            _handlers = handlers;
            _serviceProvider = serviceProvider;
            _connection = new ConnectionFactory().CreateConnection();
            _serviceName = serviceName;
            _serializer = serializer;
        }

        public async Task StartAsync()
        {
            var exchangeName = $"ex_{_serviceName}";
            using (var model = _connection.CreateModel())
            {
                model.ExchangeDeclare(exchangeName, "direct", true, false);
            }

            foreach (var handler in _handlers)
            {
                var currentModel = _models[handler.Key] = _connection.CreateModel();
                var queueResponse = currentModel.QueueDeclare($"q_{handler.Key.FullName}", false, false, true);
                currentModel.QueueBind($"q_{handler.Key.FullName}", exchangeName, handler.Key.FullName);
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
            
            var envelope = _serializer.Deserialize(typeof(Envelope<>).MakeGenericType(type), e.Body).GetAwaiter().GetResult();
            var result = service.Process(envelope);
            var message = result.GetAwaiter().GetResult();

            var props = model.CreateBasicProperties();
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("requestId", requestId);
            props.Headers = headers;
            model.BasicPublish("", responseQ, props, _serializer.Serialize(message).GetAwaiter().GetResult());

        }
    }
}
