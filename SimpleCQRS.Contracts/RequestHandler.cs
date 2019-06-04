using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public abstract class RequestHandler<TRequest, TResponse> : BaseRequestHandler, IRequestHandler<TRequest, TResponse>
    {
        public abstract Task<TResponse> Process(Envelope<TRequest> request);
        public override async Task<object> Process(object request)
        {
            return await Process((Envelope<TRequest>)request);
        }
    }
}
