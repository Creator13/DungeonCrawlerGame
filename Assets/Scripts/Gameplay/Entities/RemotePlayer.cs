using Dungen.Netcode;
using UnityEngine;

namespace Dungen.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class RemotePlayer : NetworkedBehavior
    {
        private void Awake()
        {
            controllingEntity = GetComponent<IsoEntity>();
        }
    }
}
