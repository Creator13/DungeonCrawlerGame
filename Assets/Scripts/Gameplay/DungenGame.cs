using System;
using Dungen.Gameplay.States;
using Dungen.Netcode;
using FSM;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Dungen.Gameplay
{
    public class DungenGame : MonoBehaviour
    {
        public FiniteStateMachine<DungenBlackboard> GameStateMachine { get; private set; }

        [SerializeField] private UIManager uiManager;
        [SerializeField] private ClientBehaviour clientBehaviour;

        public DungenClient Client => clientBehaviour.Client;

        private DungenBlackboard blackboard;

        private void Awake()
        {
            GameStateMachine = new FiniteStateMachine<DungenBlackboard>();
            clientBehaviour = gameObject.AddComponent<ClientBehaviour>();

            blackboard = new DungenBlackboard {
                gameController = this,
                ui = uiManager
            };
        }

        private void Start()
        {
            GameStateMachine.Initialize(new JoiningState(blackboard));
        }

        private void Update()
        {
            GameStateMachine.Update();
        }

        public void RequestStateChange<T>() where T : State<DungenBlackboard>
        {
            var newState = (T) Activator.CreateInstance(typeof(T), blackboard);

            GameStateMachine.ChangeState(newState);
        }

        public void ConnectToServer(string name, string ip)
        {
            clientBehaviour.CreateAndConnect(this, name, ip);
        }

        public void StartServerAndConnect(string name, string ip)
        {
            SceneManager.LoadScene("Server", LoadSceneMode.Additive);
            ConnectToServer(name, ip);
        }
    }
}
