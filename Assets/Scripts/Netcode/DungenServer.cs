using System.Collections.Generic;
using System.Linq;
using Dungen.Gameplay;
using Dungen.Highscore;
using Networking;
using Unity.Networking.Transport;
using UnityEngine;

namespace Dungen.Netcode
{
    public class DungenServer : Server
    {
        private static uint managedNetworkId;
        public static uint NextNetworkId => managedNetworkId++;

        protected override Dictionary<ushort, ServerMessageHandler> NetworkMessageHandlers =>
            new Dictionary<ushort, ServerMessageHandler> {
                {(ushort) DungenMessage.Handshake, lobby.AcceptConnection},
                {(ushort) DungenMessage.StartRequest, HandleStartRequest},
                {(ushort) DungenMessage.ClientReady, HandleClientReady},
                {(ushort) DungenMessage.MoveActionRequest, HandleMoveActionRequest},
                {(ushort) DungenMessage.AttackActionRequest, HandleAttackActionRequest},
            };

        private readonly Lobby lobby;
        private readonly GameSimulator simulator;
        private readonly ServerHighscoreHelper serverHighscore;

        private uint[] playerTurns;
        private int currentPlayerTurn;

        public uint CurrentTurnPlayerId => playerTurns[currentPlayerTurn];
        public bool GameStarted { get; private set; }

        public DungenServer(ushort port, GameSimulator simulator, ServerHighscoreHelper serverHighscore) : base(port,
            MessageInfo.dungenTypeMap)
        {
            lobby = new Lobby(this, 4);
            this.simulator = simulator;
            this.serverHighscore = serverHighscore;
        }

        private void HandleStartRequest(NetworkConnection connection, MessageHeader header)
        {
            // Cast into throwaway to check if the message is completely valid.
            var _ = (StartRequestMessage) header;

#if DUNGEN_NETWORK_DEBUG && UNITY_EDITOR
            SendMessage(connection, new StartRequestResponseMessage {
                status = StartRequestResponseMessage.StartRequestResponse.Accepted
            });

            SendStartData();
#else
            if (lobby.PlayerCount > 1 && !lobby.Full)
            {
                SendMessage(connection, new StartRequestResponseMessage {
                    status = StartRequestResponseMessage.StartRequestResponse.Accepted
                });

                SendStartData();
            }
            else if (lobby.PlayerCount < 2)
            {
                SendMessage(connection, new StartRequestResponseMessage {
                    status = StartRequestResponseMessage.StartRequestResponse.NotEnoughPlayers
                });
            }
            else
            {
                SendMessage(connection, new StartRequestResponseMessage {
                    status = StartRequestResponseMessage.StartRequestResponse.UndefinedFailure
                });
            }
#endif
        }

        private void HandleClientReady(NetworkConnection connection, MessageHeader header)
        {
            var msg = (ClientReadyMessage) header;

            lobby.SetReadyStatus(connection, true);

            if (lobby.AllPlayersReady)
            {
                StartGame();
            }
        }

        private void HandleMoveActionRequest(NetworkConnection connection, MessageHeader header)
        {
            var request = (MoveActionRequestMessage) header;

            var playerId = lobby.GetNetworkIdOfConnection(connection);

            if (playerId != CurrentTurnPlayerId) return;

            if (simulator.Grid.SetPlayer(playerId, request.newPosition))
            {
                var response = new MoveActionPerformedMessage {
                    networkId = playerId,
                    newPosition = request.newPosition
                };

                SendBroadcast(response);
            }

            MoveNextTurn();
        }

        private void HandleAttackActionRequest(NetworkConnection connection, MessageHeader header)
        {
            var request = (AttackActionRequestMessage) header;

            if (lobby.GetNetworkIdOfConnection(connection) != CurrentTurnPlayerId) return;

            if (simulator.TryAttack(request.attackPosition))
            {
                MoveNextTurn();
            }
        }

        private void SendStartData()
        {
            var playerData = new PlayerStartData[lobby.PlayerCount];

            var i = 0;
            foreach (var player in lobby.Players)
            {
                playerData[i] = new PlayerStartData {
                    position = simulator.GetRandomFreeGridPosition(),
                    networkId = player.networkId
                };

                simulator.Grid.InitializePlayer(playerData[i]);

                i++;
            }

            var startDataMessage = new GameStartDataMessage {
                playerData = playerData
            };

            SendBroadcast(startDataMessage, lobby.PlayerConnections, true);
        }

        private void StartGame()
        {
            if (!serverHighscore.ServerLoginRequest())
            {
                Debug.LogError("Server login failed, will not publish highscores for this session.");
            }

            lobby.ClearReadyStatus();

            GameStarted = true;

            SendBroadcast(new GameStartingMessage());

            // Do first turn
            MoveNextTurn();
        }

        public void EndGame()
        {
            GameStarted = false;

            if (serverHighscore.ServerLoggedIn)
            {
                foreach (var id in lobby.HighscoreServerIds)
                {
                    serverHighscore.SendHighscoreSubmitRequest(id, simulator.Score);
                }
            }
        }

        private void MoveNextTurn()
        {
            if (playerTurns == null)
            {
                playerTurns = lobby.Players.Select(p => p.networkId).ToArray();

                currentPlayerTurn = Random.Range(0, playerTurns.Length);
            }
            else
            {
                currentPlayerTurn = (currentPlayerTurn + 1) % playerTurns.Length;
            }

            SendBroadcast(new SetTurnMessage {playerId = playerTurns[currentPlayerTurn]});
        }
    }
}
