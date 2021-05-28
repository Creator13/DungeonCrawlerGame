using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode 
{
    public class HandshakeResponseMessage : MessageHeader
    {
		public override ushort Type => (ushort) DungenMessages.HandshakeResponse;

		public int status;
		public string playerName;
		public uint networkId;

		public override void SerializeObject(ref DataStreamWriter writer) {
			// very important to call this first
			base.SerializeObject(ref writer);

			writer.WriteFixedString128(playerName);
			writer.WriteUInt(networkId);
		}

		public override void DeserializeObject(ref DataStreamReader reader) {
			// very important to call this first
			base.DeserializeObject(ref reader);

			playerName = reader.ReadFixedString128().ToString();
			networkId = reader.ReadUInt();
		}
	}
}