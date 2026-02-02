// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 24.11.2025
// Description:
// -------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NativeWebSocket;

namespace USocket
{
    internal class WebSocketMessagePublisher<T> : IWebSocketPublisher<T>, IDisposable
    {
        private readonly WebSocket m_socket;
        private readonly IWebSocketMessageConvert m_messageConvert;

        private readonly Queue<string> m_pendingMessages = new();
        private bool m_connected;

        public WebSocketMessagePublisher(WebSocket socket, IWebSocketMessageConvert messageConvert)
        {
            m_socket = socket;
            m_messageConvert = messageConvert;

            m_socket.OnOpen += HandleOpen;
            m_socket.OnClose += HandleClose;
            m_socket.OnError += HandleError;
        }

        private void HandleOpen()
        {
            m_connected = true;

            FlushPending().Forget();
        }

        private void HandleClose(WebSocketCloseCode code)
        {
            m_connected = false;
        }

        private void HandleError(string err)
        {
            m_connected = false;
        }

        public UniTask Send(T data)
        {
            string json = m_messageConvert.Serialize(data);

            if (!m_connected || m_socket.State != WebSocketState.Open)
            {
                m_pendingMessages.Enqueue(json);
                return UniTask.CompletedTask;
            }

            return Send(json);
        }

        private async UniTask FlushPending()
        {
            while (m_pendingMessages.Count > 0 && m_socket.State == WebSocketState.Open)
            {
                string json = m_pendingMessages.Dequeue();
                await Send(json);
            }
        }

        private async UniTask Send(string json)
        {
            await m_socket.Send(m_messageConvert.ToBytes(json)).AsUniTask();
        }

        public void Dispose()
        {
            m_socket.OnOpen -= HandleOpen;
            m_socket.OnClose -= HandleClose;
            m_socket.OnError -= HandleError;
        }
    }
}