// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 25.11.2025
// Description:
// -------------------------------------------------------------------

using System;
using MessagePipe;
using PipeMediator;
using USockets.PipeMediator;
using VContainer;

namespace USockets.VContainer.PipeMediator
{
    public static class Extensions
    {
        public static void RegisterWebSocketMediatorMessage<TMessage>(this IContainerBuilder containerBuilder) where TMessage : INotification
        {
            //Register Handler
            containerBuilder.Register<WebSocketPublisherPipeMediatorWrapper<TMessage>>(Lifetime.Singleton).As<IAsyncMessageHandler<TMessage>>();

            containerBuilder.Register<WebSocketListenerMessagePipeWrapper<TMessage>>(Lifetime.Singleton).As<IDisposable>().AsSelf();

            containerBuilder.AddInitializableFor<WebSocketListenerMessagePipeWrapper<TMessage>>();
        }
    }
}