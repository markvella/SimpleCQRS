namespace SimpleCQRS.Client.Configuration
{
    internal class ConnectionConfiguration
    {
        internal ConnectionConfiguration(string hostName, int port, string virtualHost, string username, string password)
        {
            HostName = hostName;
            Port = port;
            VirtualHost = virtualHost;
            UserName = username;
            Password = password;
        }

        internal string HostName { get; private set; }
        internal int Port { get; private set; }
        internal string VirtualHost { get; private set; }
        internal string UserName { get; private set; }
        internal string Password { get; private set; }
    }
}