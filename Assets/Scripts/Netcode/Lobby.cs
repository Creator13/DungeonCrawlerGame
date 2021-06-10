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

                var playerInfo = new PlayerInfo((uint) connection.InternalId, handshake.requestedPlayerName);

                var others = Players;
                
                players[connection] = playerInfo;
                PlayersUpdated?.Invoke();

                server.MarkKeepAlive(connection.InternalId);

                // Notify other players
                var joinedMessage = new PlayerJoinedMessage {playerInfo = playerInfo};
                server.SendBroadcast(joinedMessage, toExclude: connection, reliable: true);
                
                // Send other players to newly joined player
                foreach (var player in others)
                {
                    var msg = new PlayerJoinedMessage {playerInfo = player};
                    server.SendUnicast(connection, msg);
                }

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
            
            // Notify other players
            var leftMessage = new PlayerLeftMessage {playerId = (ushort) connection.InternalId};
            server.SendBroadcast(leftMessage, toExclude: connection);
        }

        private bool ConnectionInLobby(NetworkConnection connection)
        {
            return players.ContainsKey(connection);
        }
    }
}
