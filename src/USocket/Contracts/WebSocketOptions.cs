using System.Collections.Generic;

namespace USocket
{
    public sealed record WebSocketOptions
    {
        public string Url { get; init; }

        public IReadOnlyDictionary<string, string> Headers { get; init; }

        public string SubProtocol { get; init; }
    }
}