using Newtonsoft.Json;

namespace USocket
{
    internal sealed class DefaultWebSocketMessageConverter : IWebSocketMessageConverter
    {
        public string Serialize<T>(T message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public T Deserialize<T>(string message)
        {
            return JsonConvert.DeserializeObject<T>(message);
        }
    }
}