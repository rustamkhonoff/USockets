using System.Collections.Generic;
using NativeWebSocket;

namespace USocket
{
    internal sealed class WebSocketDispatcher : IWebSocketDispatcher
    {
        private readonly HashSet<WebSocket> m_sockets = new();

        public void Add(WebSocket socket)
        {
            if (socket == null)
                return;

            m_sockets.Add(socket);
        }

        public void Remove(WebSocket socket)
        {
            if (socket == null)
                return;

            m_sockets.Remove(socket);
        }

        public void Dispatch()
        {
            foreach (WebSocket socket in m_sockets)
                socket.DispatchMessageQueue();
        }
    }
}