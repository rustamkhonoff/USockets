// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 24.11.2025
// Description:
// -------------------------------------------------------------------

using System;
using UnityEngine;

namespace USocket
{
    internal class UnityLogger : ILogger
    {
        private readonly WireConfiguration m_configuration;

        public UnityLogger(WireConfiguration configuration)
        {
            m_configuration = configuration;
        }

        public void Log(WebSocketLogLevel logLevel, string message)
        {
            if (logLevel <= m_configuration.Logging.LogLevel)
                Unity_Log(logLevel, message);
        }

        private void Unity_Log(WebSocketLogLevel level, string text)
        {
            switch (level)
            {
                case WebSocketLogLevel.None:
                    break;
                case WebSocketLogLevel.Error:
                    Debug.LogError(text);
                    break;
                case WebSocketLogLevel.Warning:
                    Debug.LogWarning(text);
                    break;
                case WebSocketLogLevel.Info:
                    Debug.Log(text);
                    break;
                case WebSocketLogLevel.Verbose:
                    Debug.Log(text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}