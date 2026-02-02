// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 24.11.2025
// Description:
// -------------------------------------------------------------------

namespace USocket
{
    public interface ILogger
    {
        void Log(WebSocketLogLevel logLevel, string message);
    }
}