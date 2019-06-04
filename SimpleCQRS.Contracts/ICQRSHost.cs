using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public interface ICQRSHost
    {
        Task StartAsync();
    }
}
