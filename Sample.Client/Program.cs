using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Formatters.Json;
using ProtoBuf.Meta;
using Sample.Contracts;
using SimpleCQRS.Client;
using SimpleCQRS.Contracts;
using SimpleCQRS.Loggers;
using SimpleCQRS.Loggers.Console;
using SimpleCQRS.Serializers.Json;

namespace Sample.Client
{
    class Program
    {
        public static ConcurrentBag<DateTime> timeStamps = new ConcurrentBag<DateTime>();
        
        static async Task Main(string[] args)
        {
            RuntimeTypeModel.Default.Add(typeof(Envelope<HelloWorldRequest>), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldRequest), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldResponse), true);
            RuntimeTypeModel.Default.CompileInPlace();
            
            var totalTime = 0L;
            var requests = 100000;
            var logger = new ConsoleLogger();
            
            var client = ClientFactory.Create<HelloWorldRequest, HelloWorldResponse>(c =>
            {
                c.ConnectTo()
                    .Using(new JsonSerializer())
                    .Using(logger)
                    .ForOperation("SampleServer", "HelloWorld")
                    .SetPoolingSize(10, 10)
                    .SetMaximumTimeout(TimeSpan.FromSeconds(60));
            });

            var metrics = new MetricsBuilder()
                .Report.ToConsole(o =>
                {
                    o.FlushInterval = TimeSpan.FromSeconds(2);
                    o.MetricsOutputFormatter = new MetricsJsonOutputFormatter();
                })
                .Build();
            
            var sentRequestsCount = new CounterOptions
            {
                Name = "Sent Requests",
                MeasurementUnit = Unit.Calls
            };
            
            var failedRequestsCount = new CounterOptions
            {
                Name = "Failed Requests",
                MeasurementUnit = Unit.Calls
            };
            
            var successfulRequestsCount = new CounterOptions
            {
                Name = "Successful Requests",
                MeasurementUnit = Unit.Calls
            };
            
            var applicationErrorCount = new CounterOptions
            {
                Name = "Application Errors",
                MeasurementUnit = Unit.Calls
            };
            
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
                        .ContinueWith(r =>
                        {
                            metrics.Measure.Counter.Increment(sentRequestsCount);

                            if (r.IsFaulted)
                            {
                                metrics.Measure.Counter.Increment(failedRequestsCount);

                                if (r.Exception != null)
                                {
                                    logger.Log(LogLevel.Error, r.Exception.Message, r.Exception);
                                }
                            }
                            else if (r.IsCompletedSuccessfully)
                            {
                                metrics.Measure.Counter.Increment(successfulRequestsCount);

                                var response = r.Result;

                                if (response == null)
                                {
                                    metrics.Measure.Counter.Increment(applicationErrorCount, "null-response");
                                }
                                else if (string.IsNullOrEmpty(response.Message))
                                {
                                    metrics.Measure.Counter.Increment(applicationErrorCount, "empty-message");
                                }
                                else if (response.Message.Equals(request.Message) == false)
                                {
                                    metrics.Measure.Counter.Increment(applicationErrorCount, "non-matching-message");
                                }
                            }
                            
                        }, cts.Token)
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
            }
            
            Task.WaitAll(metrics.ReportRunner.RunAllAsync().ToArray());
        }
    }
}
