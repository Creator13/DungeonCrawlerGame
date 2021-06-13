using Networking;

namespace Dungen.Netcode
{
    public class GameStartingMessage : MessageHeader
    {
        public override ushort Type => (ushort) DungenMessage.GameStarting;
    }
}
