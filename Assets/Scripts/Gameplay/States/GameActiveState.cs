using Dungen.Netcode;
using FSM;
using Networking;

namespace Dungen.Gameplay.States
{
    public class GameActiveState : State<DungenBlackboard>
    {
        public GameActiveState(DungenBlackboard bb) : base(bb) { }

        public override void Enter(FiniteStateMachine<DungenBlackboard> parent)
        {
            base.Enter(parent);
            
            blackboard.gameController.Client.AddHandler(DungenMessage.MoveActionPerformed, HandleMoveActionPerformed);
            blackboard.gameController.Client.AddHandler(DungenMessage.SetTurn, HandleSetTurn);
        }

        public override void Exit()
        {
            base.Exit();
            
            blackboard.gameController.Client.RemoveHandler(DungenMessage.MoveActionPerformed, HandleMoveActionPerformed);
            blackboard.gameController.Client.RemoveHandler(DungenMessage.SetTurn, HandleSetTurn);
        }

        private void HandleMoveActionPerformed(MessageHeader header)
        {
            var action = (MoveActionPerformedMessage) header;
            
            blackboard.gameController.MovePlayer(action.networkId, action.newPosition);
        }

        private void HandleSetTurn(MessageHeader header)
        {
            var msg = (SetTurnMessage) header;

            if (msg.playerId == blackboard.gameController.Client.OwnNetworkId)
            {
                blackboard.gameController.StartTurn();
            }
            else
            {
                blackboard.gameController.EndTurn();
            }
        }
    }
}
