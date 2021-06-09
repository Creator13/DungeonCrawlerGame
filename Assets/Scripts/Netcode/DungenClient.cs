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
            networkMessageHandlers ??= new Dictionary<ushort, ClientMessageHandler>() {
                {(ushort) DungenMessage.HandshakeResponse, HandleHandshakeResponse},
            };

        private readonly string originalPlayerName;
        private readonly DungenGame gameController;

        public string PlayerName { get; private set; }
        public uint NetworkID { get; private set; }
        public bool InGame { get; private set; }

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
                NetworkID = response.networkId;
                PlayerName = response.playerName;
            }

            gameController.RequestStateChange<WaitingToStartState>(); // TODO replace with event to eliminate the gameController reference?
        }

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
    }
}
