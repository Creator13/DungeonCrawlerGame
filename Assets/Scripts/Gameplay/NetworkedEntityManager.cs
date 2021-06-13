using System.Collections.Generic;
using System.Linq;
using Dungen.Netcode;
using Dungen.World;
using UnityEngine;

namespace Dungen.Gameplay
{
    public class NetworkedEntityManager : MonoBehaviour
    {
        [SerializeField] private DungenGame gameController;
        [SerializeField] private IsoGrid grid;
        public RemotePlayer remotePlayerPrefab;
        public NetworkedBehavior enemyPrefab;

        private readonly Dictionary<uint, NetworkedBehavior> behaviors = new Dictionary<uint, NetworkedBehavior>();

        [SerializeField] private List<uint> ids;

        private void Update()
        {
            ids = behaviors.Keys.ToList();
        }

        public void RegisterEntity(NetworkedBehavior behavior, uint networkId)
        {
            behaviors[networkId] = behavior;
        }
        
        public void UnregisterEntity(uint networkId)
        {
            behaviors.Remove(networkId);
        }

        public void SpawnEntity(NetworkedBehavior prototype, uint networkId, Vector2Int position)
        {
            var obj = Instantiate(prototype);
            obj.SetGrid(grid);
            obj.InitializeFromNetwork(position);

            obj.NetworkId = networkId;
            obj.name = string.IsNullOrEmpty(obj.playerName) ? obj.GetType().ToString() : obj.playerName;

            RegisterEntity(obj, networkId);
        }
        
        public void DespawnEntity(uint networkId)
        {
            var entity = behaviors[networkId];
            
            Destroy(entity.gameObject);
            behaviors.Remove(networkId);
        }

        public void MoveEntity(uint id, Vector2Int to)
        {
            behaviors[id].Move(to);
        }

        public void DespawnAll()
        {
            foreach (var id in behaviors.Keys)
            {
                Destroy(behaviors[id].gameObject);
            }
            
            behaviors.Clear();
        }
    }
}
