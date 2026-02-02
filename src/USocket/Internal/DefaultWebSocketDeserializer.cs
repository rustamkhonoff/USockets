using System.Text;
using Newtonsoft.Json;

namespace USocket
{
    internal class DefaultWebSocketMessageConvert : IWebSocketMessageConvert
    {
        public string ToJson(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public byte[] ToBytes(string json)
        {
            return Encoding.UTF8.GetBytes(json);
        }
    }
}