using UnityEngine;
using UnityEngine.Assertions;

namespace Dungen.Netcode
{
    public class ClientBehavior : MonoBehaviour
    {
        public DungenClient Client { get; private set; }

        public void Initialize(string name)
        {
            Client = new DungenClient(name);
        }

        private void OnDestroy()
        {
            Client?.Dispose();
        }

        private void Update()
        {
            Client?.Update();
        }

        public void SimpleConnect()
        {
            Initialize("Generic name");
            Connect("127.0.0.1", 1511);
        }

        public void Connect(string address, ushort port = 1511)
        {
            Assert.IsNotNull(Client, "Client was not yet initialized.");
            
            Client.Connect(address, port);
        }

        public void Disconnect()
        {
            Assert.IsNotNull(Client, "Client was not yet initialized.");
            
            Client.Disconnect();
        }
    }
}
