// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 21.11.2025
// Description:
// -------------------------------------------------------------------

using System;
using NativeWebSocket;
using UnityEngine;
using USocket;
using VContainer;
using ILogger = USocket.ILogger;

namespace USockets.VContainer
{
    public static class Extensions
    {
        public static void AddUSockets(this IContainerBuilder builder, Action<WireConfiguration> configure = null)
        {
            WireConfiguration configuration = new();
            configure?.Invoke(configuration);

            builder.RegisterInstance(configuration);
            builder.Register<WebSocketsRegistry>(Lifetime.Singleton);
            builder.Register<WebSocketsAutoRegister>(Lifetime.Singleton);

            builder.Register(configuration.MessageConverter, Lifetime.Singleton).As<IWebSocketMessageConvert>();
            builder.Register(configuration.LoggerType, Lifetime.Singleton).As<ILogger>();

            builder.AddTickableFor<WebSocketsRegistry>(a => a.DispatchMessages());

            builder.AddInitializableFor<WebSocketsAutoRegister>();
        }

        public static void RegisterWebSocket(this IContainerBuilder builder, string key, Func<WebSocket> factory)
        {
            builder.Register<WebSocketRegistryInfo>(Lifetime.Scoped)
                .As<IWebSocketRegistryInfo>()
                .WithParameter(typeof(string), key)
                .WithParameter(typeof(Func<WebSocket>), factory);
        }

        public static void RegisterWebSocketPublisher<TMessage>(this IContainerBuilder builder, string socketKey)
        {
            builder.Register(r =>
                {
                    if (r.Resolve<WebSocketsRegistry>().TryGet(socketKey, out WebSocket webSocket))
                        return new WebSocketMessagePublisher<TMessage>(webSocket, r.Resolve<IWebSocketMessageConvert>());

                    Debug.LogWarning($"There is no web socket with key {socketKey}");
                    return null;
                }, Lifetime.Singleton)
                .As<IWebSocketPublisher<TMessage>, IDisposable>();
        }

        public static void RegisterWebSocketListener<TMessage>(this IContainerBuilder builder, string socketKey, Func<string, bool> messageFilter)
        {
            builder.Register(r =>
                {
                    if (r.Resolve<WebSocketsRegistry>().TryGet(socketKey, out WebSocket webSocket))
                        return new WebSocketMessageListener<TMessage>(webSocket, r.Resolve<IWebSocketMessageConvert>(), messageFilter);

                    Debug.LogWarning($"There is no web socket with key {socketKey}");
                    return null;
                }, Lifetime.Singleton)
                .As<IWebSocketListener<TMessage>, IDisposable>();
        }
    }
}