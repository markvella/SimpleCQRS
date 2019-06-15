using ProtoBuf.Meta;
using Sample.Contracts;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleCQRS.Client;
using SimpleCQRS.Serializers.Json;

namespace Sample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(new string[] { }).GetAwaiter().GetResult();

        }
        public static System.Collections.Concurrent.ConcurrentBag<DateTime> timeStamps = new ConcurrentBag<DateTime>();
        static async Task MainAsync(string[] args)
        {
            RuntimeTypeModel.Default.Add(typeof(Envelope<HelloWorldRequest>), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldRequest), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldResponse), true);
            RuntimeTypeModel.Default.CompileInPlace();
            var totalTime = 0L;
            var requests = 100000;

            var client = ClientFactory.Create<HelloWorldRequest, HelloWorldResponse>(c =>
            {
                c.ConnectTo()
                    .Using(new JsonSerializer())
                    .ForOperation("SampleServer", "HelloWorld")
                    .SetPoolingSize(10, 10)
                    .SetMaximumTimeout(TimeSpan.FromSeconds(5));
            });

            using (client)
            {
                var cts = new CancellationTokenSource();
                var tasks = new List<Task<DateTime>>();
                var sw = new Stopwatch();
                sw.Start();
                
                for (var i = 0; i < requests; i++)
                {
                    var request = new HelloWorldRequest
                    {
                        Message = Guid.NewGuid().ToString()
                    };

                    var task = client
                        .RequestAsync(request, cts.Token)
                        .ContinueWith(r => DateTime.UtcNow, cts.Token);
 
                    tasks.Add(task);
                }
                
                var dateTimeList = Task.WhenAll(tasks).GetAwaiter().GetResult();
                sw.Stop();
                totalTime = sw.ElapsedMilliseconds;
                Console.WriteLine($"Avg response time: {totalTime / (decimal)requests}ms");
                var aggregate = dateTimeList
                    .GroupBy(x => x.ToString("ddMMyyyyHHmmss"));
                foreach (var second in aggregate)
                {
                    Console.WriteLine($"{second.Count()}req/s");
                }
                Console.ReadLine();
            }
        }
    }
}
