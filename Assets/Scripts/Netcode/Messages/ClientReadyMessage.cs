using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class ClientReadyMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.ClientReady;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
        }
    }
}
