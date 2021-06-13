using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class PlayerLeftMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.PlayerLeft;

        public uint playerId;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteUInt(playerId);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            playerId = reader.ReadUInt();
        }
    }
}
