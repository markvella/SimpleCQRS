using Sample.Contracts;
using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Server.Handlers
{
    public class HelloWorldRequestHandler : RequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        public override async Task<HelloWorldResponse> Process(Envelope<HelloWorldRequest> request)
        {
            return new HelloWorldResponse { Message = $"Hello World {request.Payload.Message}" };
        }
    }
}
