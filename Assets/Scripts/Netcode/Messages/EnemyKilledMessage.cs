using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class EnemyKilledMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.EnemyKilled;

        public uint networkId;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteUInt(networkId);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            networkId = reader.ReadUInt();
        }
    }
}
