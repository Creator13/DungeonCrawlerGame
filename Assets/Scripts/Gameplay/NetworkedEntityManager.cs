using System.Collections.Generic;
using Dungen.Netcode;
using Dungen.World;
using UnityEngine;

namespace Dungen.Gameplay
{
    public class NetworkedEntityManager : MonoBehaviour
    {
        [SerializeField] private DungenGame gameController;
        [SerializeField] private IsoGrid grid;
        public NetworkedPlayer networkedPlayerPrefab;

        private readonly Dictionary<uint, NetworkedEntity> entities = new Dictionary<uint, NetworkedEntity>();
        
        public void SpawnEntity(NetworkedEntity protoEntity, PlayerStartData startData)
        {
            var entity = Instantiate(protoEntity);
            entity.grid = grid;
            entity.NetworkId = startData.networkId;
            entity.name = gameController.Players[startData.networkId].name;
            entity.SetTileDirect(startData.position);

            entities[startData.networkId] = entity;
        }

        public void DespawnEntity(uint networkId)
        {
            var entity = entities[networkId];
            Destroy(entity.gameObject);
            entities.Remove(networkId);
        }
    }
}
