using System;
using Cysharp.Threading.Tasks;
using NativeWebSocket;

namespace USocket
{
    public interface IWebSocketClient<TIncoming, in TOutgoing> : IDisposable
    {
        event Action<TIncoming> OnMessage;
        event WebSocketOpenEventHandler OnOpen;
        event WebSocketErrorEventHandler OnError;
        event WebSocketCloseEventHandler OnClose;

        WebSocketState State { get; }

        UniTask Connect(WebSocketOptions options);
        UniTask Send(TOutgoing message);
        UniTask Close();
    }
}