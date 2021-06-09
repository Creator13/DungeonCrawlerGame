using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class StartRequestMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.StartRequest;
    }

    public class StartRequestResponseMessage : MessageHeader
    {
        public enum StartRequestResponse : byte { Accepted, NotEnoughPlayers, UndefinedFailure }

        public override ushort Type => (ushort) DungenMessage.StartRequestResponse;

        public StartRequestResponse status;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteByte((byte) status);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            status = (StartRequestResponse) reader.ReadByte();
        }
    }
}
