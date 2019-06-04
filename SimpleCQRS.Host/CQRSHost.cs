using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
            _model = _connection.CreateModel();
            foreach (var handler in _handlers)
            {
                var queueResponse = _model.QueueDeclare($"q_{handler.Key.FullName}", false, true, true);
                _model.ExchangeDeclare($"ex_{handler.Key.FullName}", "fanout", true, false);
                _model.QueueBind($"q_{handler.Key.FullName}", $"ex_{handler.Key.FullName}", "");
                var consumer = new EventingBasicConsumer(_model);
                _model.BasicConsume($"q_{handler.Key.FullName}", true, consumer);

                consumer.Received += Consumer_Received;
            }
            Console.ReadLine();


        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var type = Type.GetType(Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["type"]));
            var responseQ = Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers["responsequeue"]);
            var service = (BaseRequestHandler)_serviceProvider.GetService(_handlers[type]);
            var envelope = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Body), typeof(Envelope<>).MakeGenericType(type));
            var result = service.Process(envelope);
            var message = result.GetAwaiter().GetResult();

            var props = _model.CreateBasicProperties();
            _model.BasicPublish("", responseQ, props, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));

        }
    }
}
