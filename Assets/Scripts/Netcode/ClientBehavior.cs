using UnityEngine;

namespace Dungen.Netcode
{
    public class ClientBehavior : MonoBehaviour
    {
        private DungenClient client;
        
        private void Start()
        {
            client = new DungenClient();
        }

        private void OnDestroy()
        {
            client.Dispose();
        }

        private void Update()
        {
            client.Update();
        }

        public void Connect()
        {
            client.Connect(port: 1511);
        }
    }
}
