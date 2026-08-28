using System;
using USocket;
using USocket.VContainer;
using VContainer;

namespace USockets.VContainer
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

            builder.Register(configuration.LoggerType, Lifetime.Singleton)
                .As<USocket.ILogger>();

            builder.Register<WebSocketDispatcher>(Lifetime.Singleton)
                .As<IWebSocketDispatcher>();

            builder.Register<WebSocketClientFactory>(Lifetime.Singleton)
                .As<IWebSocketClientFactory>();

            builder.AddTickableFor<WebSocketDispatcher>(
                dispatcher => dispatcher.Dispatch()
            );
        }
    }
}