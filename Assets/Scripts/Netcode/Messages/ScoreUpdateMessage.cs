using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class ScoreUpdateMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.ScoreUpdate;

        public int newScore;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteInt(newScore);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            newScore = reader.ReadInt();
        }
    }
}
