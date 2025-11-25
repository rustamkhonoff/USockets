// -------------------------------------------------------------------
// Author: Shokhrukhkhon Rustamkhonov
// Date: 24.11.2025
// Description:
// -------------------------------------------------------------------

using System;
using VContainer;
using VContainer.Unity;

namespace USockets.VContainer
{
    internal static class WrappersExtensions
    {
        internal static void AddTickableFor<T>(this IContainerBuilder builder, Action<T> toDo)
        {
            builder.Register(r => new Wrappers<T>.TickableWrapper(r.Resolve<T>(), toDo), Lifetime.Singleton).As<ITickable>();
        }

        internal static void AddInitializableFor<T>(this IContainerBuilder builder, Action<T> toDo = null)
        {
            builder.Register(r => new Wrappers<T>.InitializableWrapper(r.Resolve<T>(), toDo), Lifetime.Singleton).As<IInitializable>();
        }
    }

    internal static class Wrappers<T>
    {
        internal sealed class InitializableWrapper : IInitializable
        {
            private readonly T m_t;
            private readonly Action<T> m_toDo;

            public InitializableWrapper(T t, Action<T> toDo)
            {
                m_t = t;
                m_toDo = toDo;
            }

            public void Initialize()
            {
                m_toDo?.Invoke(m_t);
            }
        }

        internal sealed class TickableWrapper : ITickable
        {
            private readonly T m_t;
            private readonly Action<T> m_toDo;

            public TickableWrapper(T t, Action<T> toDo)
            {
                m_t = t;
                m_toDo = toDo;
            }

            public void Tick()
            {
                m_toDo?.Invoke(m_t);
            }
        }
    }
}