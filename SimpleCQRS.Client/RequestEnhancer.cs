using System.Collections.Generic;
using RabbitMQ.Client;

namespace SimpleCQRS.Client
{
    internal class RequestEnhancer : IRequestEnhancer
    {
        private readonly IBasicProperties _props;
        
        internal RequestEnhancer(IBasicProperties props)
        {
            _props = props;
        }
        
        public IRequestEnhancer AddHeader(string key, object value)
        {
            if (_props.Headers == null)
            {
                _props.Headers = new Dictionary<string, object>();
            }
            
            _props.Headers.Add(key, value);
            return this;
        }
    }
}