using UnityEngine;

namespace Dungen.Netcode
{
    public class ServerBehavior : MonoBehaviour
    {
        private DungenServer server;

        private void Awake()
        {
            server = new DungenServer(1511);
        }

        private void Start()
        {
            server.Start();
        }

        private void Update()
        {
            server.Update();
        }

        private void OnDestroy()
        {
            server.Stop();
        }
    }
}
