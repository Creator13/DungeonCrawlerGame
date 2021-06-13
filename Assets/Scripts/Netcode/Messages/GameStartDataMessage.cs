using Networking;
using Unity.Networking.Transport;
using UnityEngine;
using Utils;

namespace Dungen.Netcode
{
    public struct PlayerStartData
    {
        public uint networkId;
        public Vector2Int position;
    }

    public class GameStartDataMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.GameStartData;

        public PlayerStartData[] playerData;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteByte((byte) playerData.Length);
            foreach (var data in playerData)
            {
                writer.WriteUInt(data.networkId);
                writer.WriteVector2Int(data.position);
            }
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            var count = reader.ReadByte();

            playerData = new PlayerStartData[count];

            for (var i = 0; i < count; i++)
            {
                playerData[i].networkId = reader.ReadUInt();
                playerData[i].position = reader.ReadVector2Int();
            }
        }
    }
}
