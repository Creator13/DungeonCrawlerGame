using Dungen.Netcode;
using Dungen.UI;
using FSM;
using Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungen.Gameplay.States
{
    public class WaitingToStartState : State
    {
        private DungenGame gameController;

        public WaitingToStartState(DungenGame game)
        {
            this.gameController = game;
        }

        public override void Enter(FiniteStateMachine parent)
        {
            base.Enter(parent);

            Debug.Log("Press space to start");

            gameController.Client.AddHandler(DungenMessage.StartRequestResponse, HandleStartRequestResponse);
        }

        public override void Execute()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                gameController.Client.RequestGameStart();
            }
        }

        public override void Exit()
        {
            base.Exit();

            gameController.Client.RemoveHandler(DungenMessage.StartRequestResponse, HandleStartRequestResponse);
        }

        private void HandleStartRequestResponse(MessageHeader header)
        {
            var response = (StartRequestResponseMessage) header;

            gameController.Modal.ShowModal(Modal.ModalDialogAction.Confirm, "Server says", $"{response.status}");
        }
    }
}
