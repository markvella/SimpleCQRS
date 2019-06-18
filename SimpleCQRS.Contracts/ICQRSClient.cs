using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public interface ICQRSClient
    {
        Task<TResponse> Request<T,TResponse>(T Request);
    }
}
