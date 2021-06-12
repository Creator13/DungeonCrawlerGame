using UnityEngine;

namespace Dungen
{
    [RequireComponent(typeof(CharacterController))]
    public class NetworkedPlayer : NetworkedEntity
    {
        public string playerName;
    }
}
