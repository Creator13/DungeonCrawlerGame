using System.Collections.Generic;
using System.Linq;
using Dungen.World;
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
            };

        private readonly GeneratorSettings generatorSettings;
        private readonly Lobby lobby;
        private readonly ServerGrid grid;

        private uint[] playerTurns;
        private int currentPlayerTurn;

        public bool GameStarted { get; private set; }

        public DungenServer(ushort port, GeneratorSettings generatorSettings) : base(port, MessageInfo.dungenTypeMap)
        {
            lobby = new Lobby(this, 4);
            this.generatorSettings = generatorSettings;

            grid = new ServerGrid(generatorSettings);
        }

        private void HandleStartRequest(NetworkConnection connection, MessageHeader header)
        {
            // Cast into throwaway to check if the message is completely valid.
            var _ = (StartRequestMessage) header;

#if DUNGEN_NETWORK_DEBUG && UNITY_EDITOR
            SendUnicast(connection, new StartRequestResponseMessage {
                status = StartRequestResponseMessage.StartRequestResponse.Accepted
            });

            SendStartData();
#else
            if (lobby.PlayerCount > 1 && !lobby.Full)
            {
                SendUnicast(connection, new StartRequestResponseMessage {
                    status = StartRequestResponseMessage.StartRequestResponse.Accepted
                });

                SendStartData();
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

            if (grid.SetPlayer(playerId, request.newPosition))
            {
                var response = new MoveActionPerformedMessage {
                    networkId = playerId,
                    newPosition = request.newPosition
                };

                SendBroadcast(response);
            }

            MoveNextTurn();
        }

        private void SendStartData()
        {
            var playerData = new PlayerStartData[lobby.PlayerCount];

            var i = 0;
            foreach (var player in lobby.Players)
            {
                playerData[i] = new PlayerStartData {
                    position = new Vector2Int(Random.Range(0, generatorSettings.sizeX), Random.Range(0, generatorSettings.sizeY)),
                    networkId = player.networkId
                };

                grid.InitializePlayer(playerData[i]);

                i++;
            }

            var startDataMessage = new GameStartDataMessage {
                playerData = playerData
            };

            SendBroadcast(startDataMessage, lobby.PlayerConnections, true);
        }

        private void StartGame()
        {
            lobby.ClearReadyStatus();

            GameStarted = true;

            SendBroadcast(new GameStartingMessage());

            MoveNextTurn();
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
