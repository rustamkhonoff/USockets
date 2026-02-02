// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 24.11.2025
// Description:
// -------------------------------------------------------------------

using System;
using PipeMediator;

namespace USocket.PipeMediator
{
    public class WebSocketListenerMessagePipeWrapper<TMessage> : IDisposable
        where TMessage : INotification
    {
        private readonly IWebSocketListener<TMessage> m_webSocketListener;
        private readonly IMediator m_mediator;

        public WebSocketListenerMessagePipeWrapper(IWebSocketListener<TMessage> webSocketListener, IMediator mediator)
        {
            m_webSocketListener = webSocketListener;
            m_mediator = mediator;

            m_webSocketListener.OnMessage += HandleMessage;
        }

        private void HandleMessage(TMessage obj)
        {
            m_mediator.Publish(obj);
        }

        public void Dispose()
        {
            m_webSocketListener.OnMessage -= HandleMessage;
        }
    }
}