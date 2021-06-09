using System;
using Dungen.Gameplay.States;
using Dungen.Netcode;
using Dungen.UI;
using FSM;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dungen.Gameplay
{
    public class DungenGame : MonoBehaviour
    {
        
        public FiniteStateMachine GameStateMachine { get; private set; }

        [SerializeField] private Modal modal;
        [SerializeField] private JoinMenuView joinMenuView;
        [SerializeField] private ClientBehaviour clientBehaviour;

        public DungenClient Client => clientBehaviour.Client;
        public Modal Modal => modal;
        
        private void Awake()
        {
            GameStateMachine = new FiniteStateMachine();
            clientBehaviour = gameObject.AddComponent<ClientBehaviour>();
        }

        private void Start()
        {
            GameStateMachine.Initialize(new JoiningState(joinMenuView, this));
        }

        private void Update()
        {
            GameStateMachine.Update();
        }

        public void RequestStateChange<T>() where T : State
        {
            var newState = (T) Activator.CreateInstance(typeof(T), new object[] {this});

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
