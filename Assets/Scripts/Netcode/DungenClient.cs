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

        public DungenClient() : base(MessageInfo.dungenTypeMap) { }

        protected override void OnConnected()
        {
            var msg = new HandshakeMessage {name = "Casper"};
            SendPackedMessage(msg);
        }

        private void HandleHandshakeResponse(MessageHeader header)
        {
            var message = (HandshakeResponseMessage) header;
            Debug.Log($"The server says: {message.message}");
        }
    }
}
