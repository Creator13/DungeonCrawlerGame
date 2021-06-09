using UnityEngine;

namespace Dungen.Netcode
{
    public class ServerBehavior : MonoBehaviour
    {
        [SerializeField] private ushort port = 1511;
        
        private DungenServer server;

        private void Awake()
        {
            server = new DungenServer(port);
        }

        private void Start()
        {
            server?.Start();
        }

        private void Update()
        {
            server?.Update();
        }

        private void OnDisable()
        {
            server?.Stop();
        }

        private void OnDestroy()
        {
            server?.Stop();
        }
    }
}
