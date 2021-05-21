using System;
using System.Collections.Generic;

namespace Networking
{
    public static class NetworkMessageInfo
    {
        public static Dictionary<ushort, Type> typeMap =
            new Dictionary<ushort, Type> {
                // {NetworkMessageType.HANDSHAKE, typeof(HandshakeMessage)},
                // {NetworkMessageType.HANDSHAKE_RESPONSE, typeof(HandshakeResponseMessage)},
                // {NetworkMessageType.CHAT_MESSAGE, typeof(ChatMessage)},
                // {NetworkMessageType.CHAT_QUIT, typeof(ChatQuitMessage)},
                // {NetworkMessageType.NETWORK_SPAWN, typeof(SpawnMessage)},
                // {NetworkMessageType.NETWORK_DESTROY, typeof(DestroyMessage)},
                // {NetworkMessageType.NETWORK_UPDATE_POSITION, typeof(UpdatePositionMessage)},
                // {NetworkMessageType.INPUT_UPDATE, typeof(InputUpdateMessage)},
                {(ushort) BuiltinMessageTypes.Ping, typeof(PingMessage)},
                {(ushort) BuiltinMessageTypes.Pong, typeof(PongMessage)}
            };
    }
}
