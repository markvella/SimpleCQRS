using System.Threading;

namespace SimpleCQRS.Client
{
    internal class ResponseObject
    {
        internal SemaphoreSlim Semaphore { get; set; }
        internal byte[] Response { get; set; }
    }
}