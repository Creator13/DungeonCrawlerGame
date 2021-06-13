using Networking;
using Unity.Networking.Transport;
using UnityEngine;

namespace Dungen.Netcode
{
    public class MoveActionRequestMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.MoveActionRequest;

        public Vector2Int newPosition;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteInt(newPosition.x);
            writer.WriteInt(newPosition.y);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            newPosition = new Vector2Int();
            newPosition.x = reader.ReadInt();
            newPosition.y = reader.ReadInt();
        }
    }
}
