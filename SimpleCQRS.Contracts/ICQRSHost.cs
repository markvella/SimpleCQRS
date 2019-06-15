using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public interface ICQRSHost
    {
        Task StartAsync();
    }
}
