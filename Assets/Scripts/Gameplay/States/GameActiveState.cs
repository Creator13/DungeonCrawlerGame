using FSM;

namespace Dungen.Gameplay.States
{
    public class GameActiveState : State<DungenBlackboard>
    {
        private readonly DungenGame gameController;

        public GameActiveState(DungenBlackboard bb) : base(bb) { }
    }
}
