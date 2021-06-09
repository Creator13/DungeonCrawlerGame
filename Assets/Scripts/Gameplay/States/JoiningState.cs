using Dungen.Netcode;
using Dungen.UI;
using FSM;
using Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungen.Gameplay.States
{
    public class JoiningState : State
    {
        private readonly JoinMenuView view;
        private readonly DungenGame gameController;

        public JoiningState(JoinMenuView view, DungenGame game)
        {
            this.view = view;
            this.gameController = game;
        }

        public override void Enter(FiniteStateMachine parent)
        {
            base.Enter(parent);
            
            view.gameObject.SetActive(true);
        }

        public override void Execute()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                gameController.StartServerAndConnect("batman", "127.0.0.1");
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            view.gameObject.SetActive(false);
        }

        public override bool ValidateTransition(State newState)
        {
            return newState.GetType() == typeof(WaitingToStartState);
        }
    }
}
