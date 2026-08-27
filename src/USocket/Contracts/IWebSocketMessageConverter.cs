namespace USocket
{
    public interface IWebSocketMessageConverter
    {
        string Serialize<T>(T message);
        T Deserialize<T>(string message);
    }
}