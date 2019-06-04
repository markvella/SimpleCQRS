using SimpleCQRS.Contracts;
using System;

namespace Sample.Contracts
{
    public class HelloWorldRequest:IRequest
    {
        public string Message { get; set; }
    }
}
