// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 25.11.2025
// Description:
// -------------------------------------------------------------------

using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using PipeMediator;

namespace USockets.PipeMediator
{
    public class WebSocketPublisherPipeMediatorWrapper<TMessage> : IAsyncMessageHandler<TMessage>
        where TMessage : INotification
    {
        private readonly IWebSocketPublisher<TMessage> m_publisher;

        public WebSocketPublisherPipeMediatorWrapper(IWebSocketPublisher<TMessage> publisher)
        {
            m_publisher = publisher;
        }

        public UniTask HandleAsync(TMessage message, CancellationToken cancellationToken)
        {
            return m_publisher.Send(message);
        }
    }
}