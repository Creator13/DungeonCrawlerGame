using Dungen.Gameplay;
using Dungen.Netcode;

namespace Gameplay.Entities
{
    public class Enemy : NetworkedBehavior
    {
        private void Awake()
        {
            controllingEntity = GetComponent<IsoEntity>();
        }
    }
}
