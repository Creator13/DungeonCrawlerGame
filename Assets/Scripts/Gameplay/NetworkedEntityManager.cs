using System.Collections.Generic;
using Dungen.Netcode;
using Dungen.World;
using Unity.Networking.Transport;
using UnityEngine;
using Utils;

namespace Dungen.Gameplay
{
    public class NetworkedEntityManager : MonoBehaviour
    {
        [SerializeField] private DungenGame gameController;
        [SerializeField] private IsoGrid grid;
        public RemotePlayer remotePlayerPrefab;

        private readonly Dictionary<uint, NetworkedBehavior> behaviors = new Dictionary<uint, NetworkedBehavior>();

        public void RegisterEntity(NetworkedBehavior behavior, uint networkId)
        {
            behaviors[networkId] = behavior;
        }

        public void SpawnEntity(NetworkedBehavior prototype, PlayerStartData startData)
        {
            var obj = Instantiate(prototype);
            obj.SetGrid(grid);
            obj.InitializeFromNetwork(startData);
            
            obj.NetworkId = startData.networkId;
            obj.playerName = gameController.Players[startData.networkId].name;
            obj.name = obj.playerName;

            RegisterEntity(obj, startData.networkId);
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
    }
}
