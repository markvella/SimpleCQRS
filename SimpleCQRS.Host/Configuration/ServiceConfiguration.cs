using System;

namespace SimpleCQRS.Host.Configuration
{
    internal class OperationConfiguration<T> : OperationConfiguration
    {
        internal OperationConfiguration(string operationName, Action<T> handler) : base(operationName, typeof(T))
        {
            Handler = handler;
        }

        internal Action<T> Handler { get; }
    }

    internal class OperationConfiguration
    {
        internal OperationConfiguration(string operationName, Type type)
        {
            OperationName = operationName;
            Type = type;
        }

        internal string OperationName { get; }

        internal Type Type { get; }
    }
}
