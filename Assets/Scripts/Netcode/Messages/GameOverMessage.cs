using Networking;
using Unity.Networking.Transport;

namespace Dungen.Netcode
{
    public class GameOverMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.GameOver;

        public int finalScore;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteInt(finalScore);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            finalScore = reader.ReadInt();
        }
    }
}
