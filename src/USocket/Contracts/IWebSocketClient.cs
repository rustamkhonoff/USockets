using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NativeWebSocket;

namespace USocket
{
    public interface IWebSocketClient<TIncoming, TOutgoing> : IDisposable
    {
        event Action<TIncoming> OnMessage;
        event Action OnOpen;
        event Action<string> OnError;
        event Action<WebSocketCloseCode> OnClose;

        WebSocketState State { get; }
        bool IsConnected { get; }

        UniTask Connect(WebSocketOptions options, CancellationToken ct = default);
        UniTask Send(TOutgoing message, CancellationToken ct = default);
        UniTask Close();
    }
}