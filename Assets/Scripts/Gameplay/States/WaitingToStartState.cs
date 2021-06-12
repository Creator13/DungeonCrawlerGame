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
            blackboard.gameController.Client.AddHandler(DungenMessage.GameStartData, HandleGameStartData);
            View.gameObject.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();

            blackboard.gameController.Client.RemoveHandler(DungenMessage.StartRequestResponse, HandleStartRequestResponse);
            blackboard.gameController.Client.RemoveHandler(DungenMessage.GameStartData, HandleGameStartData);
            View.gameObject.SetActive(false);
        }

        private void HandleStartRequestResponse(MessageHeader header)
        {
            var response = (StartRequestResponseMessage) header;

            if (response.status != StartRequestResponseMessage.StartRequestResponse.Accepted)
            {
                blackboard.ui.Modal.ShowModal(Modal.ModalDialogAction.Confirm, "Error", $"{response.status}");
            }
        }
        
        private void HandleGameStartData(MessageHeader header)
        {
            var dataMessage = (GameStartDataMessage) header;
            
            blackboard.gameController.InitializeWorld(dataMessage.playerData);
            
            blackboard.gameController.Client.SendPackedMessage(new ClientReadyMessage());
            
            blackboard.gameController.RequestStateChange<GameActiveState>();
        }

        public override bool ValidateTransition(State<DungenBlackboard> newState)
        {
            return newState is GameActiveState;
        }
    }
}
