using SimpleCQRS.Serializers;

namespace SimpleCQRS.Client.Configuration
{
    public interface IClientConfiguration
    {
        IClientConfiguration ConnectTo(
            string hostName = "localhost",
            int port = 5672,
            string virtualHost = "/",
            string username = "guest",
            string password = "guest");

        IClientConfiguration UseService(string serviceName);
        
        IClientConfiguration Using(ISerializer serializer);
    }
}