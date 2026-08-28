namespace USocket
{
    public interface IWebSocketDispatcher
    {
        void Add(NativeWebSocket.WebSocket socket);
        void Remove(NativeWebSocket.WebSocket socket);
        void Dispatch();
    }
}