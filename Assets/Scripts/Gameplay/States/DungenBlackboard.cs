using Dungen.World;
using FSM;

namespace Dungen.Gameplay.States
{
    public class DungenBlackboard : IBlackboard
    {
        public DungenGame gameController;
        public UIManager ui;
        public IsoGrid grid;
    }
}
