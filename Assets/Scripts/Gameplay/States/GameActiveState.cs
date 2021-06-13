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

            blackboard.ui.GameHudView.gameObject.SetActive(true);

            blackboard.gameController.Client.AddHandler(DungenMessage.MoveActionPerformed, HandleMoveActionPerformed);
            blackboard.gameController.Client.AddHandler(DungenMessage.EnemySpawn, HandleEnemySpawn);
            blackboard.gameController.Client.AddHandler(DungenMessage.EnemyMove, HandleEnemyMove);
            blackboard.gameController.Client.AddHandler(DungenMessage.EnemyKilled, HandleEnemyKilled);
            blackboard.gameController.Client.AddHandler(DungenMessage.SetTurn, HandleSetTurn);
            blackboard.gameController.Client.AddHandler(DungenMessage.ScoreUpdate, HandleScoreUpdate);
        }

        public override void Exit()
        {
            base.Exit();

            blackboard.ui.GameHudView.gameObject.SetActive(false);

            blackboard.gameController.Client.RemoveHandler(DungenMessage.MoveActionPerformed, HandleMoveActionPerformed);
            blackboard.gameController.Client.RemoveHandler(DungenMessage.EnemySpawn, HandleEnemySpawn);
            blackboard.gameController.Client.RemoveHandler(DungenMessage.EnemyMove, HandleEnemyMove);
            blackboard.gameController.Client.RemoveHandler(DungenMessage.EnemyKilled, HandleEnemyKilled);
            blackboard.gameController.Client.RemoveHandler(DungenMessage.SetTurn, HandleSetTurn);
            blackboard.gameController.Client.RemoveHandler(DungenMessage.ScoreUpdate, HandleScoreUpdate);
        }

        private void HandleMoveActionPerformed(MessageHeader header)
        {
            var action = (MoveActionPerformedMessage) header;

            blackboard.gameController.MoveEntity(action.networkId, action.newPosition);
        }

        private void HandleSetTurn(MessageHeader header)
        {
            var msg = (SetTurnMessage) header;

            blackboard.gameController.SetTurn(msg.playerId);
        }

        private void HandleEnemySpawn(MessageHeader header)
        {
            var msg = (EnemySpawnMessage) header;
            
            blackboard.gameController.SpawnEnemy(msg.networkId, msg.position);
        }

        private void HandleEnemyMove(MessageHeader header)
        {
            var msg = (EnemyMoveMessage) header;

            blackboard.gameController.MoveEntity(msg.networkId, msg.position);
        }

        private void HandleEnemyKilled(MessageHeader header)
        {
            var msg = (EnemyKilledMessage) header;

            blackboard.gameController.RemoveEntity(msg.networkId);
        }

        private void HandleScoreUpdate(MessageHeader header)
        {
            var msg = (ScoreUpdateMessage) header;

            blackboard.gameController.UpdateScore(msg.newScore);
        }
    }
}
