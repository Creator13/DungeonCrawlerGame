using Dungen.Gameplay;
using UnityEngine;

namespace Dungen.Netcode
{
    public class ClientBehaviour : MonoBehaviour
    {
        public DungenClient Client { get; private set; }

        public void CreateAndConnect(DungenGame gameController, string name, string address, ushort port = 1511)
        {
            Client = new DungenClient(name, gameController); // TODO gamecotnroller shouldnt be here
            Client.Connect(address, port);
        }

        private void OnDestroy()
        {
            Client?.Dispose();
        }

        private void Update()
        {
            Client?.Update();
        }
    }
}
