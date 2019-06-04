using Microsoft.Extensions.DependencyInjection;
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
