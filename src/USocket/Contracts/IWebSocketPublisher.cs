// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 25.11.2025
// Description:
// -------------------------------------------------------------------

using Cysharp.Threading.Tasks;

namespace USocket
{
    public interface IWebSocketPublisher<in T>
    {
        UniTask Send(T data);
    }
}