using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Meta;
using Sample.Contracts;
using Sample.Server.Handlers;
using SimpleCQRS.Contracts;
using SimpleCQRS.Host;
using System;
using System.Threading.Tasks;

namespace Sample.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            RuntimeTypeModel.Default.Add(typeof(Envelope<HelloWorldRequest>), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldRequest), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldResponse), true);
            RuntimeTypeModel.Default.CompileInPlace();
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IRequestHandler<HelloWorldRequest, HelloWorldResponse>, HelloWorldRequestHandler>();
            using (var provider = serviceProvider.BuildServiceProvider())
            {
                new HostBuilder()
                    .AddHandler<HelloWorldRequest, IRequestHandler<HelloWorldRequest, HelloWorldResponse>>()
                    .BindServiceProvider(provider)
                    .Build().StartAsync().GetAwaiter().GetResult();
            }

        }
    }
}
