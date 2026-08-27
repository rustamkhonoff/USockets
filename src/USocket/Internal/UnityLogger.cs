using System;
using UnityEngine;

namespace USocket
{
    internal sealed class UnityLogger : ILogger
    {
        private readonly WireConfiguration m_configuration;

        public UnityLogger(WireConfiguration configuration)
        {
            m_configuration = configuration;
        }

        public void Log(WebSocketLogLevel logLevel, string message)
        {
            if (logLevel > m_configuration.Logging.LogLevel)
                return;

            Write(logLevel, message);
        }

        private void Write(WebSocketLogLevel level, string message)
        {
            switch (level)
            {
                case WebSocketLogLevel.None:
                    break;

                case WebSocketLogLevel.Error:
                    Debug.LogError(message);
                    break;

                case WebSocketLogLevel.Warning:
                    Debug.LogWarning(message);
                    break;

                case WebSocketLogLevel.Info:
                case WebSocketLogLevel.Verbose:
                    Debug.Log(message);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}