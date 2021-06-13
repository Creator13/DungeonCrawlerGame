using Networking;
using Unity.Networking.Transport;
using UnityEngine;
using Utils;

namespace Dungen.Netcode
{
    public class AttackActionRequestMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.AttackActionRequest;

        public Vector2Int attackPosition;
        
        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            
            writer.WriteVector2Int(attackPosition);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            attackPosition = reader.ReadVector2Int();
        }
    }
}
