using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class HandshakeResponseMessage : MessageHeader
    {
        public enum HandshakeResponseStatus : byte { Accepted, GameInProgress, LobbyFull }

        public override ushort Type => (ushort) DungenMessage.HandshakeResponse;

        public HandshakeResponseStatus status;
        public string playerName;
        public uint networkId;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteFixedString128(playerName);
            writer.WriteUInt(networkId);
            writer.WriteByte((byte) status);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            playerName = reader.ReadFixedString128().ToString();
            networkId = reader.ReadUInt();
            status = (HandshakeResponseStatus) reader.ReadByte();
        }
    }
}
