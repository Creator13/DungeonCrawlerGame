using System.Collections.Generic;
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

        private readonly GeneratorSettings generatorSettings;
        private readonly Lobby lobby;

        protected override Dictionary<ushort, ServerMessageHandler> NetworkMessageHandlers =>
            new Dictionary<ushort, ServerMessageHandler> {
                {(ushort) DungenMessage.Handshake, lobby.AcceptConnection},
                {(ushort) DungenMessage.StartRequest, HandleStartRequest},
                {(ushort) DungenMessage.ClientReady, HandleClientReady}
            };

        public DungenServer(ushort port, GeneratorSettings generatorSettings) : base(port, MessageInfo.dungenTypeMap)
        {
            lobby = new Lobby(this, 4);
            this.generatorSettings = generatorSettings;
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
        }

        private void HandleClientReady(NetworkConnection connection, MessageHeader header)
        {
            var msg = (ClientReadyMessage) header;
            
            lobby.SetReadyStatus(connection, true);
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
                i++;
            }
            
            var startDataMessage = new GameStartDataMessage {
                playerData = playerData
            };
            
            SendBroadcast(startDataMessage, lobby.PlayerConnections, true);
        }

        private void StartGame()
        {
            
        }
    }
}
