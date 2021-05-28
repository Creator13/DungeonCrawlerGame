using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace Dungen.Netcode
{
    public class DungenClient : Client
    {
        protected override Dictionary<ushort, ClientMessageHandler> NetworkMessageHandlers =>
            new Dictionary<ushort, ClientMessageHandler> {
                {(ushort) DungenMessages.HandshakeResponse, HandleHandshakeResponse}
            };

        private readonly string originalPlayerName;
        
        public string PlayerName { get; private set; }
        public uint NetworkID { get; private set; }

        public DungenClient(string playerName) : base(MessageInfo.dungenTypeMap)
        {
            originalPlayerName = playerName;
        }

        protected override void OnConnected()
        {
            var handshake = new HandshakeMessage {requestedPlayerName = originalPlayerName};
            SendPackedMessage(handshake);
        }

        private void HandleHandshakeResponse(MessageHeader header)
        {
            var response = (HandshakeResponseMessage) header;
            if (response.status >= 0)
            {
                NetworkID = response.networkId;
                PlayerName = response.playerName;
            }
        }
    }
}
