namespace USocket
{
    public sealed class WebSocketClientFactory : IWebSocketClientFactory
    {
        private readonly IWebSocketMessageConverter m_converter;
        private readonly ILogger m_logger;
        private readonly IWebSocketDispatcher m_dispatcher;

        public WebSocketClientFactory(
            IWebSocketMessageConverter converter,
            ILogger logger,
            IWebSocketDispatcher dispatcher
        )
        {
            m_converter = converter;
            m_logger = logger;
            m_dispatcher = dispatcher;
        }

        public IWebSocketClient<TIncoming, TOutgoing> Create<TIncoming, TOutgoing>()
        {
            return new WebSocketClient<TIncoming, TOutgoing>(
                m_converter,
                m_logger,
                m_dispatcher
            );
        }
    }
}