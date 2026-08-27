using System;
using NativeWebSocket;

namespace USocket
{
    internal class WebSocketLifetimeListener : IDisposable
    {
        private readonly ILogger m_logger;

        public string Key { get; }
        public WebSocket WebSocket { get; }

        public WebSocketLifetimeListener(WebSocket socket, string key, ILogger logger)
        {
            m_logger = logger;
            Key = key;
            WebSocket = socket;
            WebSocket.OnError += HandleError;
            WebSocket.OnOpen += HandleOpen;
            WebSocket.OnClose += HandleClose;
        }

        private void HandleClose(WebSocketCloseCode code)
        {
            m_logger.Log(WebSocketLogLevel.Info, $"[WebSocket] Key: {Key} Status: Closed Cause: {code.ToString()}");
        }

        private void HandleOpen()
        {
            m_logger.Log(WebSocketLogLevel.Info, $"[WebSocket] Key: {Key} Status: Open");
        }

        private void HandleError(string message)
        {
            m_logger.Log(WebSocketLogLevel.Error, $"[WebSocket] Key: {Key} Message: {message}");
        }

        public void Dispose()
        {
            m_logger.Log(WebSocketLogLevel.Info, $"[WebSocket] Key: {Key} Status: Disposing");

            WebSocket.OnError -= HandleError;
            WebSocket.OnOpen -= HandleOpen;
            WebSocket.OnClose -= HandleClose;
        }
    }
}