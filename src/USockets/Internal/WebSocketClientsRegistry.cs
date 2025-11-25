// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 24.11.2025
// Description:
// -------------------------------------------------------------------

using System;
using System.Collections.Generic;
using NativeWebSocket;

namespace USockets
{
    public class WireConfiguration
    {
        public LoggingOptions Logging { get; } = new();
        public Type LoggerType { get; } = typeof(UnityLogger);
        public Type MessageConverter { get; } = typeof(DefaultWebSocketMessageConvert);
    }

    public class LoggingOptions
    {
        public WebSocketLogLevel LogLevel { get; set; } = WebSocketLogLevel.Error;
    }

    public enum WebSocketLogLevel
    {
        None = 0,
        Verbose = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
    }

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

    internal class WebSocketsRegistry : IDisposable
    {
        private readonly ILogger m_logger;
        private readonly Dictionary<string, WebSocketEntry> m_entries = new();

        public WebSocketsRegistry(ILogger logger)
        {
            m_logger = logger;
        }

        public bool Register(string key, WebSocket socket)
        {
            if (socket == null)
                throw new InvalidOperationException("Trying to register NULL socket with key {key}, aborting");

            WebSocketLifetimeListener listener = new(socket, key, m_logger);

            return m_entries.TryAdd(key, new WebSocketEntry(key, socket, listener));
        }

        public bool TryGet(string key, out WebSocket socket)
        {
            socket = null;
            if (m_entries.TryGetValue(key, out WebSocketEntry webSocket))
            {
                socket = webSocket.Socket;
                return true;
            }

            return false;
        }

        public void DispatchMessages()
        {
            foreach (WebSocketEntry socketsValue in m_entries.Values)
                socketsValue.Socket.DispatchMessageQueue();
        }

        public void Close(string key)
        {
            if (!m_entries.TryGetValue(key, out WebSocketEntry socket)) return;

            socket.Socket.CancelConnection();
            socket.Socket.Close();
            socket.Dispose();

            m_entries.Remove(key);
        }

        public void Dispose()
        {
            foreach (WebSocketEntry entry in m_entries.Values)
                entry.Dispose();
        }
    }
}