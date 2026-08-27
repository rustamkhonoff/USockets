namespace USocket
{
    internal sealed class WebSocketClientFactory : IWebSocketClientFactory
    {
        private readonly IWebSocketMessageConverter m_converter;
        private readonly ILogger m_logger;

        public WebSocketClientFactory(
            IWebSocketMessageConverter converter,
            ILogger logger
        )
        {
            m_converter = converter;
            m_logger = logger;
        }

        public IWebSocketClient<TIncoming, TOutgoing> Create<TIncoming, TOutgoing>()
        {
            return new WebSocketClient<TIncoming, TOutgoing>(
                m_converter,
                m_logger
            );
        }
    }
}