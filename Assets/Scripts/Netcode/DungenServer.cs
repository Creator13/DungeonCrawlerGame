using System.Collections.Generic;
using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class DungenServer : Server
    {
        public readonly Lobby lobby;

        protected override Dictionary<ushort, ServerMessageHandler> NetworkMessageHandlers =>
            new Dictionary<ushort, ServerMessageHandler> {
                {(ushort) DungenMessage.Handshake, lobby.AcceptConnection},
                {(ushort) DungenMessage.StartRequest, HandleStartRequest},
            };

        public DungenServer(ushort port) : base(port, MessageInfo.dungenTypeMap)
        {
            lobby = new Lobby(this, 4);
        }

        private void HandleStartRequest(NetworkConnection connection, MessageHeader header)
        {
            // Cast into throwaway to check if the message is completely valid.
            var _ = (StartRequestMessage) header;

            if (lobby.PlayerCount > 1 && !lobby.Full)
            {
                SendUnicast(connection, new StartRequestResponseMessage {
                    status = StartRequestResponseMessage.StartRequestResponse.Accepted
                });

                // TODO send game start messages
            }
            else if (lobby.PlayerCount < 2)
            {
                SendUnicast(connection, new StartRequestResponseMessage {
                    status = StartRequestResponseMessage.StartRequestResponse.NotEnoughPlayers
                });
            }
            else
            {
                SendUnicast(connection, new StartRequestResponseMessage {
                    status = StartRequestResponseMessage.StartRequestResponse.UndefinedFailure
                });
            }
        }
    }
}
