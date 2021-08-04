using System;
using System.Collections.Generic;
using Dungen.Gameplay;
using Dungen.Gameplay.States;
using Networking;

namespace Dungen.Netcode
{
    public class DungenClient : Client
    {
        private Dictionary<ushort, ClientMessageHandler> networkMessageHandlers;

        protected override Dictionary<ushort, ClientMessageHandler> NetworkMessageHandlers =>
            networkMessageHandlers ??= new Dictionary<ushort, ClientMessageHandler> {
                {(ushort) DungenMessage.HandshakeResponse, HandleHandshakeResponse},
                {(ushort) DungenMessage.PlayerJoined, HandlePlayerJoined},
                {(ushort) DungenMessage.PlayerLeft, HandlePlayerLeft},
            };

        private readonly string originalPlayerName;
        private readonly DungenGame gameController;

        public PlayerInfo PlayerInfo { get; private set; }

        public uint OwnNetworkId => PlayerInfo.networkId;

        public event Action<PlayerInfo> PlayerJoined;
        public event Action<uint> PlayerLeft;

        public DungenClient(string playerName, DungenGame gameController) : base(MessageInfo.dungenTypeMap)
        {
            originalPlayerName = playerName;
            this.gameController = gameController;
        }

        protected override void OnConnected()
        {
            var handshake = new HandshakeMessage
            {
                requestedPlayerName = originalPlayerName, 
                highscoreServerId = gameController.PlayerHighscoreHelper.CurrentUser.id
            };
            SendMessage(handshake);
        }

        protected override void OnDisconnected() { }

        public void RequestGameStart()
        {
            var startRequest = new StartRequestMessage();
            SendMessage(startRequest);
        }

        private void HandleHandshakeResponse(MessageHeader header)
        {
            var response = (HandshakeResponseMessage) header;
            if (response.status >= 0)
            {
                PlayerInfo = new PlayerInfo(response.networkId, response.playerName);
            }

            gameController.RequestStateChange<WaitingToStartState>(); // TODO replace with event to eliminate the gameController reference?
        }

        private void HandlePlayerJoined(MessageHeader header)
        {
            var message = (PlayerJoinedMessage) header;
            PlayerJoined?.Invoke(message.playerInfo);
        }

        private void HandlePlayerLeft(MessageHeader header)
        {
            var message = (PlayerLeftMessage) header;
            PlayerLeft?.Invoke(message.playerId);
        }


        #region Dynamic Handlers

        public void AddHandler(DungenMessage messageType, ClientMessageHandler handler)
        {
            if (!NetworkMessageHandlers.ContainsKey((ushort) messageType))
            {
                NetworkMessageHandlers.Add((ushort) messageType, handler);
            }
            else
            {
                NetworkMessageHandlers[(ushort) messageType] += handler;
            }
        }

        public void RemoveHandler(DungenMessage messageType, ClientMessageHandler handler)
        {
            NetworkMessageHandlers[(ushort) messageType] -= handler;
            if (NetworkMessageHandlers[(ushort) messageType] == null)
            {
                NetworkMessageHandlers.Remove((ushort) messageType);
            }
        }

        #endregion
    }
}
