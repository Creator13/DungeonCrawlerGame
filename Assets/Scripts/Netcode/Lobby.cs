using System;
using System.Collections.Generic;
using System.Linq;
using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class Lobby
    {
        private readonly int lobbyCapacity;
        private readonly List<NetworkConnection> connections = new List<NetworkConnection>();
        private readonly DungenServer server;

        public Lobby(DungenServer server, int capacity)
        {
            this.server = server;
            lobbyCapacity = capacity;
        }

        public void AcceptConnection(NetworkConnection connection, MessageHeader header)
        {
            if (ConnectionInLobby(connection))
            {
                throw new InvalidOperationException(
                    $"Cannot add connection to lobby that is already in lobby. (id {connection.InternalId})");
            }

            if (connections.Count >= lobbyCapacity)
            {
                connections.Add(connection);
            }
            
            var handshakeResponse = new HandshakeResponseMessage {
                message = connections.Count >= lobbyCapacity ? "Rejected" : "Welcome!"
            };
            
            server.SendUnicast(connection, handshakeResponse);
        }

        private bool ConnectionInLobby(NetworkConnection connection)
        {
            return connections.Any(c => c.InternalId == connection.InternalId);
        }
    }
}
