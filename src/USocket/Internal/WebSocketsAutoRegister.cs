using System.Collections.Generic;
using NativeWebSocket;

namespace USocket
{
    internal class WebSocketsAutoRegister
    {
        public WebSocketsAutoRegister(WebSocketsRegistry registry, IEnumerable<IWebSocketRegistryInfo> infos)
        {
            foreach (IWebSocketRegistryInfo info in infos)
            {
                WebSocket webSocket = info.Factory();
                registry.Register(info.Key, webSocket);
                webSocket.Connect();
            }
        }
    }
}