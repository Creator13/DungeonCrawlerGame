using System.Collections.Generic;
using Networking;

namespace Dungen.Netcode
{
    public class DungenServer : Server
    {
        private readonly Lobby lobby;

        protected override Dictionary<ushort, ServerMessageHandler> NetworkMessageHandlers =>
            new Dictionary<ushort, ServerMessageHandler> {
                {(ushort) DungenMessages.Handshake, lobby.AcceptConnection},
            };

        public DungenServer(ushort port) : base(port, MessageInfo.dungenTypeMap)
        {
            lobby = new Lobby(this, 4);
        }
    }
}
