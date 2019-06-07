using ProtoBuf.Meta;
using Sample.Contracts;
using SimpleCQRS;
using SimpleCQRS.Contracts;
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
            RuntimeTypeModel.Default.Add(typeof(Envelope<HelloWorldRequest>), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldRequest), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldResponse), true);
            RuntimeTypeModel.Default.CompileInPlace();
            using (var client = new CQRSClient())
            {
                Stopwatch sw = new Stopwatch();
                for (int i = 0; i < 1000; i++)
                {
                    if (i == 1)
                        sw.Start();
                    var response = await client.Request<HelloWorldRequest, HelloWorldResponse>(new HelloWorldRequest { Message = Guid.NewGuid().ToString() });
                    Console.WriteLine(response.Message);
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds / 999);
                Console.ReadLine();
            }
        }
    }
}
