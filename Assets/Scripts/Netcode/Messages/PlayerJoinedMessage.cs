using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class PlayerJoinedMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.PlayerJoined;

        public PlayerInfo playerInfo;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteUInt(playerInfo.playerId);
            writer.WriteFixedString32(playerInfo.name);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            var id = reader.ReadUInt();
            var name = reader.ReadFixedString32().ToString();

            playerInfo = new PlayerInfo(id, name);
        }
    }
}
