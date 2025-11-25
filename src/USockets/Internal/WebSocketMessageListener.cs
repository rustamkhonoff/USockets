using System;
using NativeWebSocket;
using Newtonsoft.Json;

namespace USockets
{
    internal class WebSocketMessageListener<T> : IWebSocketListener<T>, IDisposable
    {
        public event Action<T> OnMessage;

        public event WebSocketErrorEventHandler OnError
        {
            add => m_webSocket.OnError += value;
            remove => m_webSocket.OnError -= value;
        }

        public event WebSocketOpenEventHandler OnOpen
        {
            add => m_webSocket.OnOpen += value;
            remove => m_webSocket.OnOpen -= value;
        }

        public event WebSocketCloseEventHandler OnClose
        {
            add => m_webSocket.OnClose += value;
            remove => m_webSocket.OnClose -= value;
        }

        private readonly WebSocket m_webSocket;
        private readonly IWebSocketMessageConvert m_messageConvert;
        private readonly Func<string, bool> m_filter;

        public WebSocketMessageListener(WebSocket webSocket, IWebSocketMessageConvert messageConvert, Func<string, bool> filter)
        {
            m_webSocket = webSocket;
            m_messageConvert = messageConvert;
            m_filter = filter;
            m_webSocket.OnMessage += HandleMessage;
        }

        private void HandleMessage(byte[] bytes)
        {
            string json = m_messageConvert.ToJson(bytes);

            if (!m_filter(json)) return;
            T data = m_messageConvert.Deserialize<T>(json);
            if (data == null) throw new JsonSerializationException();
            OnMessage?.Invoke(data);
        }

        public void Dispose()
        {
            m_webSocket.OnMessage -= HandleMessage;
        }
    }
}