using Dungen.UI;
using FSM;

namespace Dungen.Gameplay.States
{
    public class JoiningState : State<DungenBlackboard>
    {
        public JoiningState(DungenBlackboard blackboard) : base(blackboard) { }

        private JoinMenuView View => blackboard.ui.JoinMenuView;

        public override void Enter(FiniteStateMachine<DungenBlackboard> parent)
        {
            base.Enter(parent);

            View.gameObject.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();

            View.gameObject.SetActive(false);
        }

        public override bool ValidateTransition(State<DungenBlackboard> newState)
        {
            return newState.GetType() == typeof(WaitingToStartState);
        }
    }
}
