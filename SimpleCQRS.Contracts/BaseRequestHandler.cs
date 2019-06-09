using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public abstract class BaseRequestHandler
    {
        public abstract Task<object> Process(object request);
    }
}
