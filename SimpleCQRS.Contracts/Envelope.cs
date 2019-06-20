namespace SimpleCQRS.Contracts
{
    public class Envelope<T>
    {
        public T Message { get; set; }

        public string MessageType { get { return typeof(T).FullName; } }
        
        public string MessageId { get; set; }
        
        public string ReplyTo { get; set; }
        
        public string RoutingKey { get; set; }
        
        public string ConsumerTag { get; set; }
    }
}
