using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class HandshakeMessage : MessageHeader
    {
		public override ushort Type => (ushort) DungenMessages.Handshake;

		public string name = "";
		public uint networkId = 0;

		public override void SerializeObject(ref DataStreamWriter writer) {
			// very important to call this first
			base.SerializeObject(ref writer);

			writer.WriteFixedString128(name);
		}

		public override void DeserializeObject(ref DataStreamReader reader) {
			// very important to call this first
			base.DeserializeObject(ref reader);

			name = reader.ReadFixedString128().ToString();
		}
	}
}