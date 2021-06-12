using FSM;

namespace Dungen.Gameplay.States
{
    public class GameActiveState : State<DungenBlackboard>
    {
        public GameActiveState(DungenBlackboard bb) : base(bb) { }

        public override void Enter(FiniteStateMachine<DungenBlackboard> parent)
        {
            base.Enter(parent);
        }
    }
}
