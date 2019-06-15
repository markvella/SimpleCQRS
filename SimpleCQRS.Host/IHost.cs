using System;
using System.Threading.Tasks;

namespace SimpleCQRS.Host
{
    public interface IHost : IDisposable
    {
        Task StartAsync();

        Task StopAsync();
    }
}
