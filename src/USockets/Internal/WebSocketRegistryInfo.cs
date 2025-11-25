using System;
using NativeWebSocket;
using UnityEngine;

namespace USockets
{
    internal interface IWebSocketRegistryInfo
    {
        string Key { get; }
        Func<WebSocket> Factory { get; }
    }

    [Serializable]
    internal sealed class WebSocketRegistryInfo : IWebSocketRegistryInfo
    {
        [field: SerializeField] public string Key { get; private set; }
        [field: SerializeField] public Func<WebSocket> Factory { get; private set; }

        public WebSocketRegistryInfo(string key, Func<WebSocket> factory)
        {
            Key = key;
            Factory = factory;
        }
    }
}