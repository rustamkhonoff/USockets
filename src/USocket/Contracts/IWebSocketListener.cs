using System;
using NativeWebSocket;

namespace USocket
{
    public interface IWebSocketListener<out T>
    {
        event Action<T> OnMessage;
        event WebSocketErrorEventHandler OnError;
        event WebSocketOpenEventHandler OnOpen;
        event WebSocketCloseEventHandler OnClose;
    }
}