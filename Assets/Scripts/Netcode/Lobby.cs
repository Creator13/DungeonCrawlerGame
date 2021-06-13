﻿using System;
using System.Collections.Generic;
using System.Linq;
using Networking;
using Unity.Networking.Transport;
using UnityEngine;

namespace Dungen.Netcode
{
    public class LobbiedPlayer
    {
        public PlayerInfo playerInfo;
        public bool sentData;
        public bool ready;
    }

    public class Lobby
    {
        public event Action PlayersUpdated;

        private readonly int lobbyCapacity;
        private readonly DungenServer server;

        private readonly Dictionary<NetworkConnection, LobbiedPlayer> players = new Dictionary<NetworkConnection, LobbiedPlayer>();

        public int MaxPlayers => lobbyCapacity;
        public int PlayerCount => players.Count;
        public PlayerInfo[] Players => players.Values.Select(p => p.playerInfo).ToArray();
        public IEnumerable<NetworkConnection> PlayerConnections => players.Keys;
        public bool Full => PlayerCount >= MaxPlayers;
        public bool AllPlayersReady => players.Values.All(p => p.ready);

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
                throw new InvalidOperationException($"Cannot add connection to lobby that is already in lobby. (id {connection.InternalId})");
            }

            var networkId = DungenServer.NextNetworkId;

            HandshakeResponseMessage handshakeResponse;
            if (!Full)
            {
                var playerInfo = new PlayerInfo(networkId, handshake.requestedPlayerName);

                handshakeResponse = new HandshakeResponseMessage {
                    status = HandshakeResponseMessage.HandshakeResponseStatus.Accepted,
                    playerName = handshake.requestedPlayerName,
                    networkId = playerInfo.networkId
                };

                var others = Players;

                players[connection] = new LobbiedPlayer {playerInfo = playerInfo};
                PlayersUpdated?.Invoke();

                server.MarkKeepAlive(connection);

                // Notify all players
                var joinedMessage = new PlayerJoinedMessage {playerInfo = playerInfo};
                server.SendBroadcast(joinedMessage, reliable: true);

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
                    networkId = networkId
                };
            }

            server.SendUnicast(connection, handshakeResponse);
        }

        public uint GetNetworkIdOfConnection(NetworkConnection connection)
        {
            return players[connection].playerInfo.networkId;
        }

        public void SetReadyStatus(NetworkConnection connection, bool ready)
        {
            players[connection].ready = true;
        }

        public void ClearReadyStatus()
        {
            foreach (var p in players.Values)
            {
                p.ready = false;
            }
        }

        private void RemovePlayer(NetworkConnection connection)
        {
            if (!ConnectionInLobby(connection))
            {
                Debug.LogWarning(
                    $"Player associated with connection {connection.InternalId} was not in lobby, this may be caused when the lobby " +
                    "reacts to a client disconnecting that never got into the lobby (due to it being full for example)");
                return;
            }

            var playerName = players[connection].playerInfo.name;

            players.Remove(connection);

            server.UnmarkKeepAlive(connection);
            server.DisconnectClient(connection);

            PlayersUpdated?.Invoke();

            Debug.Log($"{playerName} left the game.");

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
