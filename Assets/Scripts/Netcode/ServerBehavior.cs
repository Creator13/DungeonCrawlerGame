using Dungen.Gameplay;
using Dungen.Highscore;
using UnityEngine;
using UnityEngine.Assertions;

namespace Dungen.Netcode
{
    public class ServerBehavior : MonoBehaviour
    {
        [SerializeField] private ushort port = 1511;
        [SerializeField] private GameSimulator simulator;
        [SerializeField] private HighscoreServerAuthenticator authenticator;

        public DungenServer Server { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(simulator);
            Server = new DungenServer(port, simulator, authenticator);
        }

        private void Start()
        {
            Server?.Start();
        }

        private void Update()
        {
            Server?.Update();
        }

        private void OnDisable()
        {
            Server?.Stop();
        }

        private void OnDestroy()
        {
            Server?.Stop();
        }
    }
}
