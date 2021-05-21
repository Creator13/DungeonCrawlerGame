using System.Collections.Generic;
using Networking;

namespace Dungen.Netcode
{
    public class DungenClient : Client
    {
        protected override Dictionary<ushort, ClientMessageHandler> NetworkMessageHandlers { get; }

        public DungenClient()
        {
            NetworkMessageHandlers = new Dictionary<ushort, ClientMessageHandler> {
                {(ushort) DungenMessages.HandshakeResponse, HandleHandshakeResponse}
            };
        }
        
        protected override void OnConnected() { }

        private void HandleHandshakeResponse(Client client, MessageHeader header)
        {
            
        }
    }
}
