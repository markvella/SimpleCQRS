using System;
using System.Threading.Tasks;
using ProtoBuf.Meta;
using Sample.Contracts;
using SimpleCQRS.Contracts;
using SimpleCQRS.Host;
using SimpleCQRS.Loggers;
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

            var logger = new ConsoleLogger();
            
            var host = HostFactory.Create(c =>
            {
                c.SetService("SampleServer")
                .ConnectTo()
                .Using(new JsonSerializer())
                .Using(logger)
                .AddOperation<HelloWorldRequest, HelloWorldResponse>("HelloWorld", (env, caller) =>
                {
                    var message = env.Message;

                    if (env.Message == null)
                    {
                        logger.Log(LogLevel.Error, "Message was null");
                    }
                    else if (string.IsNullOrWhiteSpace(env.Message.Message))
                    {
                        logger.Log(LogLevel.Error, "Message data was null or empty");
                    }
                    
                    var reply = new HelloWorldResponse
                    {
                        Message = message?.Message
                    };
                    
                    caller.SendReply(env, reply);
                });
            });

            await host.StartAsync();

            Console.ReadLine();

            await host.StopAsync();
        }
    }
}
