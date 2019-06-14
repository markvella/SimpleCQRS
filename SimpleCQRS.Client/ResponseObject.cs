using System.Threading;

namespace SimpleCQRS.Client
{
    public class ResponseObject
    {
        public SemaphoreSlim Semaphore { get; set; }
        public byte[] Response { get; set; }
    }
}