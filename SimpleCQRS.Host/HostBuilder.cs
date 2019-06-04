using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;

namespace SimpleCQRS.Host
{
    public class HostBuilder
    {
        private Dictionary<Type, Type> _handlers = new Dictionary<Type, Type>();
        private IServiceProvider _serviceProvider;

        public HostBuilder AddHandler<TRequest, TRequestHandler>() where TRequestHandler:IRequestHandler
        {
            _handlers.Add(typeof(TRequest), typeof(TRequestHandler));
            return this;
        }
        public HostBuilder BindServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return this;
        }
        public ICQRSHost Build()
        {
            return new CQRSHost(_handlers, _serviceProvider);
        }
    }
}
