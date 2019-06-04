using Sample.Contracts;
using SimpleCQRS;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(new string[] { }).GetAwaiter().GetResult();

        }
        static async Task MainAsync(string[] args)
        {
            using (var client = new CQRSClient())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < 1000; i++)
                {
                    var response = await client.Request<HelloWorldRequest, HelloWorldResponse>(new HelloWorldRequest { Message = Guid.NewGuid().ToString() });
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds/1000);
                Console.ReadLine();
            }
        }
    }
}
