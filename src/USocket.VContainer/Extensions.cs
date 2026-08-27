using System;
using USocket;
using VContainer;

namespace USocket.VContainer
{
    public static class Extensions
    {
        public static void AddUSockets(
            this IContainerBuilder builder,
            Action<WireConfiguration> configure = null
        )
        {
            WireConfiguration configuration = new();

            configure?.Invoke(configuration);

            builder.RegisterInstance(configuration);

            builder.Register(configuration.MessageConverter, Lifetime.Singleton)
                .As<IWebSocketMessageConverter>();

            builder.Register<WebSocketClientFactory>(Lifetime.Singleton)
                .As<IWebSocketClientFactory>();

            builder.Register(configuration.LoggerType, Lifetime.Singleton)
                .As<ILogger>();
        }
    }
}