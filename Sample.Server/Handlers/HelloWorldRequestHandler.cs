using System.Threading.Tasks;
using Sample.Contracts;
using SimpleCQRS.Contracts;

namespace Sample.Server.Handlers
{
    public class HelloWorldRequestHandler : RequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        public override async Task<HelloWorldResponse> Process(Envelope<HelloWorldRequest> request)
        {
            return new HelloWorldResponse { Message = $"Hello World {request.Message.Message}" };
        }
    }
}
