using Networking;
using Unity.Networking.Transport;
using UnityEngine;
using Utils;

namespace Dungen.Netcode
{
    public class MoveActionPerformedMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.MoveActionPerformed;

        public uint networkId;
        public Vector2Int newPosition;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteUInt(networkId);
            writer.WriteVector2Int(newPosition);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            networkId = reader.ReadUInt();
            newPosition = reader.ReadVector2Int();
        }
    }
}
