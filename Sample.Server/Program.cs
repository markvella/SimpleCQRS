using System;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf.Meta;
using Sample.Contracts;
using SimpleCQRS.Contracts;
using SimpleCQRS.Host;
using SimpleCQRS.Loggers.Console;
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
                .Using(new ConsoleLogger())
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

            Thread.Sleep(Timeout.Infinite);

            await host.StopAsync();
        }
    }
}
