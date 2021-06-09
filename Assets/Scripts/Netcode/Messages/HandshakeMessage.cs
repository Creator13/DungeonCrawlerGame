using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class HandshakeMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.Handshake;

        public string requestedPlayerName = "";

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            // very important to call this first
            base.SerializeObject(ref writer);

            writer.WriteFixedString128(requestedPlayerName);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            // very important to call this first
            base.DeserializeObject(ref reader);

            requestedPlayerName = reader.ReadFixedString128().ToString();
        }
    }
}
