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
using App.Metrics.Gauge;
using App.Metrics.Timer;
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
        private static readonly ILogger Logger = new ConsoleLogger();

        private static readonly CounterOptions SentRequestsCount = new CounterOptions {
            Name = "Sent Requests",
            MeasurementUnit = Unit.Calls
        };
            
        private static readonly CounterOptions FailedRequestsCount = new CounterOptions {
            Name = "Failed Requests",
            MeasurementUnit = Unit.Calls
        };
            
        private static readonly CounterOptions SuccessfulRequestsCount = new CounterOptions {
            Name = "Successful Requests",
            MeasurementUnit = Unit.Calls
        };
            
        private static readonly CounterOptions ApplicationErrorCount = new CounterOptions {
            Name = "Application Errors",
            MeasurementUnit = Unit.Calls
        };
        
        private static readonly TimerOptions RequestTimer = new TimerOptions {
            Name = "Request Timer",
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds
        };
        
        static Task Main(string[] args)
        {
            RuntimeTypeModel.Default.Add(typeof(Envelope<HelloWorldRequest>), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldRequest), true);
            RuntimeTypeModel.Default.Add(typeof(HelloWorldResponse), true);
            RuntimeTypeModel.Default.CompileInPlace();
            
            var requests = 100000;
            
            var client = ClientFactory.Create<HelloWorldRequest, HelloWorldResponse>(c =>
            {
                c.ConnectTo()
                    .Using(new JsonSerializer())
                    .Using(Logger)
                    .ForOperation("SampleServer", "HelloWorld")
                    .SetPoolingSize(10, 10)
                    .SetMaximumTimeout(TimeSpan.FromMilliseconds(50));
            });

            var metrics = new MetricsBuilder()
                .Report.ToConsole(o =>
                {
                    o.FlushInterval = TimeSpan.FromSeconds(2);
                    o.MetricsOutputFormatter = new MetricsJsonOutputFormatter();
                })
                .Build();
            
            using (client)
            {
                var cts = new CancellationTokenSource();
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cts.Token,
                    MaxDegreeOfParallelism = 30
                };

                Parallel.For(0, requests, parallelOptions, index =>
                {
                    var request = new HelloWorldRequest
                    {
                        Message = Guid.NewGuid().ToString()
                    };

                    using (metrics.Measure.Timer.Time(RequestTimer))
                    {
                        client
                            .RequestAsync(request, cts.Token)
                            .ContinueWith(r => HandleResponse(r, request, metrics), cts.Token)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                    }
                });
            }
            
            Task.WaitAll(metrics.ReportRunner.RunAllAsync().ToArray());
            
            return Task.CompletedTask;
        }

        private static void HandleResponse(Task<HelloWorldResponse> task, HelloWorldRequest request, IMetricsRoot metrics)
        {
            metrics.Measure.Counter.Increment(SentRequestsCount);

            if (task.IsFaulted)
            {
                metrics.Measure.Counter.Increment(FailedRequestsCount);

                if (task.Exception != null)
                {
                    Logger.Log(LogLevel.Error, task.Exception.Message, task.Exception);
                }
            }
            else if (task.IsCompletedSuccessfully)
            {
                metrics.Measure.Counter.Increment(SuccessfulRequestsCount);

                var response = task.Result;

                if (response == null)
                {
                    metrics.Measure.Counter.Increment(ApplicationErrorCount, "null-response");
                }
                else if (string.IsNullOrEmpty(response.Message))
                {
                    metrics.Measure.Counter.Increment(ApplicationErrorCount, "empty-message");
                }
                else if (response.Message.Equals(request.Message) == false)
                {
                    metrics.Measure.Counter.Increment(ApplicationErrorCount, "non-matching-message");
                }
            }
        }
    }
}
