using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<uint, PlayerInfo> others = new Dictionary<uint, PlayerInfo>();

        public PlayerInfo PlayerInfo { get; private set; }
        public bool InGame { get; private set; }

        public List<PlayerInfo> Players
        {
            get
            {
                var players = others.Values.ToList();
                players.Add(PlayerInfo);
                return players;
            }
        }

        public DungenClient(string playerName, DungenGame gameController) : base(MessageInfo.dungenTypeMap)
        {
            originalPlayerName = playerName;
            this.gameController = gameController;
        }

        protected override void OnConnected()
        {
            var handshake = new HandshakeMessage {requestedPlayerName = originalPlayerName};
            SendPackedMessage(handshake);
        }

        protected override void OnDisconnected()
        {
            InGame = false;
        }

        public void RequestGameStart()
        {
            var startRequest = new StartRequestMessage();
            SendPackedMessage(startRequest);
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

            others[message.playerInfo.playerId] = message.playerInfo;
        }

        private void HandlePlayerLeft(MessageHeader header)
        {
            var message = (PlayerLeftMessage) header;

            others.Remove(message.playerId);
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
        }

        #endregion
    }
}
