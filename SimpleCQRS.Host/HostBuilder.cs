using SimpleCQRS.Contracts;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleCQRS.Host
{
    public class HostBuilder
    {
        private Dictionary<Type, Type> _handlers = new Dictionary<Type, Type>();
        private IServiceProvider _serviceProvider;
        private string _serviceName;
        private Type _serializerType;

        public HostBuilder AddHandler<TRequest, TRequestHandler>() where TRequestHandler:IRequestHandler
        {
            _handlers.Add(typeof(TRequest), typeof(TRequestHandler));
            return this;
        }

        public HostBuilder WithServiceName(string service)
        {
            _serviceName = service;
            return this;
        }

        public HostBuilder UsingSerializer<T>() where T : ISerializer
        {
            _serializerType = typeof(T);
            return this;
        }
        public HostBuilder BindServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return this;
        }
        public ICQRSHost Build()
        {
            return new CQRSHost(_handlers, _serviceProvider, (ISerializer)_serviceProvider.GetRequiredService(typeof(ISerializer)), _serviceName);
        }
    }
}
