using Networking;
using Unity.Networking.Transport;
using UnityEngine;
using Utils;

namespace Dungen.Netcode
{
    public class EnemySpawnMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.EnemySpawn;

        public uint networkId;
        public Vector2Int position;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteUInt(networkId);
            writer.WriteVector2Int(position);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            networkId = reader.ReadUInt();
            position = reader.ReadVector2Int();
        }
    }
}
