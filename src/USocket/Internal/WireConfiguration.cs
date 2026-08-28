using System;

namespace USocket
{
    public sealed class WireConfiguration
    {
        public LoggingOptions Logging { get; } = new();

        public Type LoggerType { get; set; } = typeof(UnityLogger);

        public Type MessageConverter { get; set; } =
            typeof(DefaultWebSocketMessageConverter);
    }
}