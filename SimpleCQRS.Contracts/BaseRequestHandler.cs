using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public abstract class BaseRequestHandler
    {
        public abstract Task<object> Process(object request);
    }
}
