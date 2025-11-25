namespace USockets
{
    public interface IWebSocketMessageConvert
    {
        string ToJson(byte[] bytes);
        T Deserialize<T>(string json);
        
        string Serialize<T>(T data);
        byte[] ToBytes(string json);
    }
}