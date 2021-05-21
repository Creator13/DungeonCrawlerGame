using System.Collections.Generic;
using Networking;
using Unity.Networking.Transport;
using UnityEngine.Assertions;

namespace Dungen.Netcode
{
    public class Lobby
    {
        private int lobbyCapacity;
        private List<NetworkConnection> connections;
        private readonly DungenServer server;

        public Lobby(DungenServer server)
        {
            this.server = server;
        }
        
        public void AcceptConnection(NetworkConnection connection, MessageHeader header)
        {
            
            
        }
    }
}
