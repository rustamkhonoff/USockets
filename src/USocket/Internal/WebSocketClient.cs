using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using NativeWebSocket;

namespace USocket
{
    internal sealed class WebSocketClient<TIncoming, TOutgoing> :
        IWebSocketClient<TIncoming, TOutgoing>
    {
        public event Action<TIncoming> OnMessage;
        public event WebSocketOpenEventHandler OnOpen;
        public event WebSocketErrorEventHandler OnError;
        public event WebSocketCloseEventHandler OnClose;

        public WebSocketState State => m_socket?.State ?? WebSocketState.Closed;

        private readonly IWebSocketMessageConverter m_converter;
        private readonly ILogger m_logger;

        private WebSocket m_socket;
        private bool m_disposed;

        public WebSocketClient(
            IWebSocketMessageConverter converter,
            ILogger logger
        )
        {
            m_converter = converter;
            m_logger = logger;
        }

        public async UniTask Connect(WebSocketOptions options)
        {
            ThrowIfDisposed();

            if (m_socket != null)
                await Close();

            Dictionary<string, string> headers = options.Headers == null
                ? null
                : new Dictionary<string, string>(options.Headers);

            m_socket = string.IsNullOrEmpty(options.SubProtocol)
                ? new WebSocket(options.Url, headers)
                : new WebSocket(options.Url, options.SubProtocol, headers);

            m_socket.OnOpen += HandleOpen;
            m_socket.OnMessage += HandleMessage;
            m_socket.OnError += HandleError;
            m_socket.OnClose += HandleClose;

            m_logger.Log(
                WebSocketLogLevel.Info,
                $"[WebSocket] Connecting: {options.Url}"
            );

            await m_socket.Connect().AsUniTask();
        }

        public UniTask Send(TOutgoing message)
        {
            ThrowIfDisposed();

            if (m_socket == null)
                throw new InvalidOperationException("WebSocket is not created.");

            if (m_socket.State != WebSocketState.Open)
                throw new InvalidOperationException(
                    $"WebSocket is not open. Current state: {m_socket.State}");

            string json = m_converter.Serialize(message);

            m_logger.Log(
                WebSocketLogLevel.Verbose,
                $"[WebSocket] Send: {json}"
            );

            return m_socket.SendText(json).AsUniTask();
        }

        public async UniTask Close()
        {
            if (m_socket == null)
                return;

            WebSocket socket = m_socket;
            m_socket = null;

            Unsubscribe(socket);

            m_logger.Log(
                WebSocketLogLevel.Info,
                "[WebSocket] Closing"
            );

            if (socket.State == WebSocketState.Connecting)
            {
                socket.CancelConnection();
                return;
            }

            if (socket.State == WebSocketState.Open)
                await socket.Close().AsUniTask();
        }

        private void HandleOpen()
        {
            m_logger.Log(
                WebSocketLogLevel.Info,
                "[WebSocket] Connected"
            );

            OnOpen?.Invoke();
        }

        private void HandleMessage(byte[] bytes)
        {
            try
            {
                string json = Encoding.UTF8.GetString(bytes);

                m_logger.Log(
                    WebSocketLogLevel.Verbose,
                    $"[WebSocket] Receive: {json}"
                );

                TIncoming message = m_converter.Deserialize<TIncoming>(json);

                if (message == null)
                    throw new InvalidOperationException(
                        $"Failed to deserialize {typeof(TIncoming).Name}.");

                OnMessage?.Invoke(message);
            }
            catch (Exception e)
            {
                m_logger.Log(
                    WebSocketLogLevel.Error,
                    $"[WebSocket] Deserialize error: {e}"
                );

                OnError?.Invoke(e.Message);
            }
        }

        private void HandleError(string error)
        {
            m_logger.Log(
                WebSocketLogLevel.Error,
                $"[WebSocket] Error: {error}"
            );

            OnError?.Invoke(error);
        }

        private void HandleClose(WebSocketCloseCode code)
        {
            m_logger.Log(
                WebSocketLogLevel.Info,
                $"[WebSocket] Closed: {code}"
            );

            OnClose?.Invoke(code);
        }

        private void Unsubscribe(WebSocket socket)
        {
            socket.OnOpen -= HandleOpen;
            socket.OnMessage -= HandleMessage;
            socket.OnError -= HandleError;
            socket.OnClose -= HandleClose;
        }

        public void Dispose()
        {
            if (m_disposed)
                return;

            m_disposed = true;

            if (m_socket != null)
            {
                Unsubscribe(m_socket);

                m_socket.CancelConnection();
                m_socket.Close();

                m_socket = null;
            }

            OnMessage = null;
            OnOpen = null;
            OnError = null;
            OnClose = null;
        }

        private void ThrowIfDisposed()
        {
            if (m_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}