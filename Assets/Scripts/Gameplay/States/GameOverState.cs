using FSM;

namespace Dungen.Gameplay.States
{
    public class GameOverState : State<DungenBlackboard>
    {
        public GameOverState(DungenBlackboard bb) : base(bb) { }

        public override void Enter(FiniteStateMachine<DungenBlackboard> parent)
        {
            base.Enter(parent);
            
            blackboard.ui.GameOverView.gameObject.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();
            blackboard.ui.GameOverView.gameObject.SetActive(false);
        }

        public override bool ValidateTransition(State<DungenBlackboard> newState)
        {
            return newState is JoiningState;
        }
    }
}
