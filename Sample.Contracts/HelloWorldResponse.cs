using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public class HelloWorldResponse:IResponse
    {
        public string Message { get; set; }
    }
}
