using System;
using System.Collections.Generic;
using System.Linq;
using Networking;
using Unity.Networking.Transport;
using UnityEngine;

namespace Dungen.Netcode
{
    public class Lobby
    {
        public event Action PlayersUpdated;

        private readonly int lobbyCapacity;
        private readonly DungenServer server;

        private readonly Dictionary<NetworkConnection, PlayerInfo> players =
            new Dictionary<NetworkConnection, PlayerInfo>();

        public int MaxPlayers => lobbyCapacity;
        public int PlayerCount => players.Count;
        public PlayerInfo[] Players => players.Values.ToArray();
        public bool Full => PlayerCount >= MaxPlayers;

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
            if (!Full)
            {
                handshakeResponse = new HandshakeResponseMessage {
                    status = HandshakeResponseMessage.HandshakeResponseStatus.Accepted,
                    playerName = handshake.requestedPlayerName,
                    networkId = (uint) connection.InternalId
                };

                players[connection] = new PlayerInfo {
                    name = handshake.requestedPlayerName
                };
                PlayersUpdated?.Invoke();

                server.MarkKeepAlive(connection.InternalId);
                
                Debug.Log($"{handshake.requestedPlayerName} joined the lobby!");
            }
            else
            {
                handshakeResponse = new HandshakeResponseMessage {
                    status = HandshakeResponseMessage.HandshakeResponseStatus.LobbyFull,
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

            players.Remove(connection);

            server.UnmarkKeepAlive(connection);
            server.DisconnectClient(connection);

            PlayersUpdated?.Invoke();
        }

        private bool ConnectionInLobby(NetworkConnection connection)
        {
            return players.ContainsKey(connection);
        }
    }
}
