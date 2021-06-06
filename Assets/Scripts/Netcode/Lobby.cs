using System;
using System.Collections.Generic;
using System.Linq;
using Networking;
using UI;
using Unity.Networking.Transport;
using UnityEngine;

namespace Dungen.Netcode
{
    public class Lobby
    {
        public event Action PlayersUpdated;

        private readonly int lobbyCapacity;
        private readonly List<NetworkConnection> connections = new List<NetworkConnection>();
        private readonly DungenServer server;
        public readonly List<Player> players = new List<Player>();

        public int MaxPlayers => lobbyCapacity;

        public Lobby(DungenServer server, int capacity)
        {
            this.server = server;
            lobbyCapacity = capacity;

            this.server.ConnectionRemoved += RemovePlayer;
        }

        public void AcceptConnection(NetworkConnection connection, MessageHeader header)
        {
            var handshake = (HandshakeMessage) header;

            if (ConnectionInLobby(connection))
            {
                throw new InvalidOperationException(
                    $"Cannot add connection to lobby that is already in lobby. (id {connection.InternalId})");
            }

            HandshakeResponseMessage handshakeResponse;
            if (connections.Count < lobbyCapacity)
            {
                connections.Add(connection);
                
                handshakeResponse = new HandshakeResponseMessage {
                    status = 0,
                    playerName = handshake.requestedPlayerName,
                    networkId = (uint) connection.InternalId
                };
                
                players.Add(new Player() {
                    active = true,
                    color = Color.clear,
                    id = connection.InternalId,
                    name = handshake.requestedPlayerName
                });
                PlayersUpdated?.Invoke();
                
                server.MarkKeepAlive(connection.InternalId);
            }
            else
            {
                handshakeResponse = new HandshakeResponseMessage {
                    status = -1,
                    playerName = handshake.requestedPlayerName,
                    networkId = (uint) connection.InternalId
                };
            }
            server.SendUnicast(connection, handshakeResponse);
        }

        private void RemovePlayer(NetworkConnection connection)
        {
            if (!ConnectionInLobby(connection))
            {
                throw new InvalidOperationException(
                    $"Cannot remove connection from lobby that is not already in lobby. (id {connection.InternalId})");
            }
            
            var player = players.First(p => p.id == connection.InternalId);
            players.Remove(player);

            server.UnmarkKeepAlive(connection);
            server.DisconnectClient(connection);
            
            var conn = connections.First(c => c.InternalId == connection.InternalId);
            connections.Remove(conn);
            PlayersUpdated?.Invoke();
        }

        private bool ConnectionInLobby(NetworkConnection connection)
        {
            return connections.Any(c => c.InternalId == connection.InternalId);
        }
    }
}
