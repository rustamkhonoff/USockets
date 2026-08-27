namespace USocket
{
    public interface IWebSocketClientFactory
    {
        IWebSocketClient<TIncoming, TOutgoing> Create<TIncoming, TOutgoing>();
    }
}