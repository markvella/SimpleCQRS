namespace SimpleCQRS.Client
{
    public interface IRequestEnhancer
    {
        IRequestEnhancer AddHeader(string key, object value);
    }
}