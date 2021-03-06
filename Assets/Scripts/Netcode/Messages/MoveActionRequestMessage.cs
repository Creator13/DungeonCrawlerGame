using Networking;
using Unity.Networking.Transport;
using UnityEngine;
using Utils;

namespace Dungen.Netcode
{
    public class MoveActionRequestMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.MoveActionRequest;

        public Vector2Int newPosition;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteVector2Int(newPosition);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            newPosition = reader.ReadVector2Int();
        }
    }
}
