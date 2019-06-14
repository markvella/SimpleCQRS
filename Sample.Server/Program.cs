using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Meta;
using Sample.Contracts;
using Sample.Server.Handlers;
using SimpleCQRS.Contracts;
using SimpleCQRS.Host;
using SimpleCQRS.Serializers.Json;

namespace Sample.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            RuntimeTypeModel.Default.Add(typeof(Envelope<HelloWorldRequest>), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldRequest), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldResponse), true);
            RuntimeTypeModel.Default.CompileInPlace();

            var host = HostFactory.Create(c =>
            {
                c.SetService("SampleServer")
                .ConnectTo()
                .Using(new JsonSerializer())
                .AddOperation<HelloWorldRequest, HelloWorldResponse>("HelloWorld", (env, caller) =>
                {
                    var message = env.Message;
                    var reply = new HelloWorldResponse
                    {
                        Message = message?.Message
                    };
                    
                    caller.SendReply(env, reply);
                });
            });

            await host.StartAsync();

            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IRequestHandler<HelloWorldRequest, HelloWorldResponse>, HelloWorldRequestHandler>();
            using (var provider = serviceProvider.BuildServiceProvider())
            {
                await new HostBuilder()
                    .AddHandler<HelloWorldRequest, IRequestHandler<HelloWorldRequest, HelloWorldResponse>>()
                    .BindServiceProvider(provider)
                    .Build()
                    .StartAsync();
            }

            await host.StopAsync();
        }
    }
}
