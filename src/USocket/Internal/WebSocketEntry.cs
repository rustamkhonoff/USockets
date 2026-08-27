using System;
using NativeWebSocket;

namespace USocket
{
    internal class WebSocketEntry : IDisposable
    {
        public string Key { get; }
        public WebSocket Socket { get; }
        public WebSocketLifetimeListener Listener { get; }

        public WebSocketEntry(string key, WebSocket socket, WebSocketLifetimeListener listener)
        {
            Key = key;
            Socket = socket;
            Listener = listener;
        }

        public void Dispose()
        {
            Listener.Dispose();
            Socket?.CancelConnection();
            Socket?.Close();
        }
    }
}