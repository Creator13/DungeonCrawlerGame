using Dungen.Netcode;
using Dungen.UI;
using FSM;
using Networking;
using UnityEngine;

namespace Dungen.Gameplay.States
{
    public class WaitingToStartState : State<DungenBlackboard>
    {
        private WaitingToStartView View => blackboard.ui.WaitingToStartView;
        
        public WaitingToStartState(DungenBlackboard bb) : base(bb) { }

        public override void Enter(FiniteStateMachine<DungenBlackboard> parent)
        {
            base.Enter(parent);

            Debug.Log("Press space to start");

            blackboard.gameController.Client.AddHandler(DungenMessage.StartRequestResponse, HandleStartRequestResponse);
            View.gameObject.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();

            blackboard.gameController.Client.RemoveHandler(DungenMessage.StartRequestResponse, HandleStartRequestResponse);
            View.gameObject.SetActive(false);
        }

        private void HandleStartRequestResponse(MessageHeader header)
        {
            var response = (StartRequestResponseMessage) header;

            if (response.status != StartRequestResponseMessage.StartRequestResponse.Accepted)
            {
                blackboard.ui.Modal.ShowModal(Modal.ModalDialogAction.Confirm, "Error", $"{response.status}");
                return;
            }

            blackboard.gameController.RequestStateChange<GameActiveState>();
        }

        public override bool ValidateTransition(State<DungenBlackboard> newState)
        {
            return newState is GameActiveState;
        }
    }
}
