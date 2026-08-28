using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using NativeWebSocket;

namespace USocket
{
    internal sealed class WebSocketClient<TIncoming, TOutgoing> :
        IWebSocketClient<TIncoming, TOutgoing>
    {
        public event Action<TIncoming> OnMessage;
        public event Action OnOpen;
        public event Action<string> OnError;
        public event Action<WebSocketCloseCode> OnClose;

        public WebSocketState State => m_socket?.State ?? WebSocketState.Closed;
        public bool IsConnected => State == WebSocketState.Open;

        private readonly IWebSocketMessageConverter m_converter;
        private readonly ILogger m_logger;
        private readonly IWebSocketDispatcher m_dispatcher;

        private WebSocket m_socket;

        private int m_connectionVersion;
        private bool m_disposed;

        public WebSocketClient(
            IWebSocketMessageConverter converter,
            ILogger logger,
            IWebSocketDispatcher dispatcher
        )
        {
            m_converter = converter;
            m_logger = logger;
            m_dispatcher = dispatcher;
        }

        public async UniTask Connect(
            WebSocketOptions options,
            CancellationToken ct = default
        )
        {
            ThrowIfDisposed();

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.Url))
                throw new ArgumentException("WebSocket URL is empty.", nameof(options));

            if (m_socket != null)
                await Close();

            ct.ThrowIfCancellationRequested();

            Dictionary<string, string> headers = options.Headers == null
                ? null
                : new Dictionary<string, string>(options.Headers);

            List<string> subProtocols = options.SubProtocols == null
                ? null
                : new List<string>(options.SubProtocols);

            WebSocket socket = subProtocols is { Count: > 0 }
                ? new WebSocket(options.Url, subProtocols, headers)
                : new WebSocket(options.Url, headers);

            m_socket = socket;

            int version = ++m_connectionVersion;

            socket.OnOpen += () => HandleOpen(version);
            socket.OnMessage += bytes => HandleMessage(version, bytes);
            socket.OnError += error => HandleError(version, error);
            socket.OnClose += code => HandleClose(version, code);

            UniTaskCompletionSource completionSource = new();

            void ConnectionOpened()
            {
                completionSource.TrySetResult();
            }

            void ConnectionError(string error)
            {
                completionSource.TrySetException(
                    new InvalidOperationException(error)
                );
            }

            void ConnectionClosed(WebSocketCloseCode code)
            {
                completionSource.TrySetException(
                    new InvalidOperationException(
                        $"WebSocket closed before connection completed: {code}")
                );
            }

            socket.OnOpen += ConnectionOpened;
            socket.OnError += ConnectionError;
            socket.OnClose += ConnectionClosed;

            using CancellationTokenRegistration registration = ct.Register(() =>
            {
                socket.CancelConnection();
                completionSource.TrySetCanceled(ct);
            });

            m_dispatcher.Add(socket);

            m_logger.Log(
                WebSocketLogLevel.Info,
                $"[WebSocket] Connecting: {options.Url}"
            );

            _ = socket.Connect();

            try
            {
                await completionSource.Task;
            }
            catch
            {
                m_dispatcher.Remove(socket);

                if (ReferenceEquals(m_socket, socket))
                    m_socket = null;

                throw;
            }
            finally
            {
                socket.OnOpen -= ConnectionOpened;
                socket.OnError -= ConnectionError;
                socket.OnClose -= ConnectionClosed;
            }
        }

        public async UniTask Send(
            TOutgoing message,
            CancellationToken ct = default
        )
        {
            ThrowIfDisposed();
            ct.ThrowIfCancellationRequested();

            if (m_socket == null)
                throw new InvalidOperationException("WebSocket is not created.");

            if (m_socket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException(
                    $"WebSocket is not open. State: {m_socket.State}");
            }

            string json = m_converter.Serialize(message);

            m_logger.Log(
                WebSocketLogLevel.Verbose,
                $"[WebSocket] Send: {json}"
            );

            await m_socket.SendText(json).AsUniTask();

            ct.ThrowIfCancellationRequested();
        }

        public async UniTask Close()
        {
            if (m_socket == null)
                return;

            WebSocket socket = m_socket;
            m_socket = null;

            m_connectionVersion++;

            m_dispatcher.Remove(socket);

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

        private void HandleOpen(int version)
        {
            if (!IsCurrent(version))
                return;

            m_logger.Log(
                WebSocketLogLevel.Info,
                "[WebSocket] Connected"
            );

            OnOpen?.Invoke();
        }

        private void HandleMessage(int version, byte[] bytes)
        {
            if (!IsCurrent(version))
                return;

            try
            {
                string json = Encoding.UTF8.GetString(bytes);

                m_logger.Log(
                    WebSocketLogLevel.Verbose,
                    $"[WebSocket] Receive: {json}"
                );

                TIncoming message = m_converter.Deserialize<TIncoming>(json);

                if (message == null)
                {
                    throw new InvalidOperationException(
                        $"Failed to deserialize {typeof(TIncoming).Name}.");
                }

                OnMessage?.Invoke(message);
            }
            catch (Exception e)
            {
                m_logger.Log(
                    WebSocketLogLevel.Error,
                    $"[WebSocket] Message error: {e}"
                );

                OnError?.Invoke(e.Message);
            }
        }

        private void HandleError(int version, string error)
        {
            if (!IsCurrent(version))
                return;

            m_logger.Log(
                WebSocketLogLevel.Error,
                $"[WebSocket] Error: {error}"
            );

            OnError?.Invoke(error);
        }

        private void HandleClose(int version, WebSocketCloseCode code)
        {
            if (!IsCurrent(version))
                return;

            m_logger.Log(
                WebSocketLogLevel.Info,
                $"[WebSocket] Closed: {code}"
            );

            OnClose?.Invoke(code);
        }

        private bool IsCurrent(int version)
        {
            return !m_disposed && version == m_connectionVersion;
        }

        public void Dispose()
        {
            if (m_disposed)
                return;

            m_disposed = true;
            m_connectionVersion++;

            WebSocket socket = m_socket;
            m_socket = null;

            if (socket != null)
            {
                m_dispatcher.Remove(socket);

                if (socket.State == WebSocketState.Connecting)
                {
                    socket.CancelConnection();
                }
                else if (socket.State == WebSocketState.Open)
                {
                    _ = socket.Close();
                }
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