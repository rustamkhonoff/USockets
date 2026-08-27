namespace USocket
{
    public interface IWebSocketRuntimeRegistry
    {
        void Add(IWebSocketRuntimeChannel channel);
        void Remove(IWebSocketRuntimeChannel channel);
    }
}