using ProtoBuf.Meta;
using Sample.Contracts;
using SimpleCQRS;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SimpleCQRS.Serializers.Protobuf;

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
            long totalTime = 0;
            int requests = 100000;
            using (var client = new CQRSClient(new ProtobufSerializer(), "HelloWorldSample"))
            {
                List<Task<DateTime>> tasks = new List<Task<DateTime>>();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < requests; i++)
                {
                    tasks.Add(RunTask(client));
                }
                var dateTimeList = Task.WhenAll<DateTime>(tasks).GetAwaiter().GetResult();
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

        public static async Task<DateTime> RunTask(ICQRSClient client)
        {
            var response = await client.Request<HelloWorldRequest, HelloWorldResponse>(new HelloWorldRequest { Message = Guid.NewGuid().ToString() });
            return DateTime.Now;
        }
    }
}
