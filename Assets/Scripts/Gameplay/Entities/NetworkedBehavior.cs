using Dungen.Gameplay;
using Dungen.World;
using UnityEngine;

namespace Dungen.Netcode
{
    public class NetworkedBehavior : MonoBehaviour
    {
        public string playerName;
        protected IsoEntity controllingEntity;

        public uint NetworkId { get; set; }

        public void InitializeFromNetwork(Vector2Int position)
        {
            controllingEntity.SetTile(position);
        }

        public void Move(Vector2Int to)
        {
            controllingEntity.MoveOverPath(to);
        }

        public void SetGrid(IsoGrid grid)
        {
            controllingEntity.grid = grid;
        }
    }
}
